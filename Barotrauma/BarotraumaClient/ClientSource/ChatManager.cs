using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
    // TODO decide what folder this falls under. Utils? GUI? or just leave where it is. Also probably a better class name name [<- no need to create a separate namespace, maybe a folder?]
    /// <summary>
    /// A class used for handling special key actions in chat boxes.
    /// For example tab completion or up/down arrow key history.
    /// </summary>
    public class ChatManager
    {
        private readonly bool loop;

        // Maximum items we want to store in the history
        private readonly short maxCount = 10;

        // List of previously stored messages
        private readonly List<string> messageList = new List<string> { string.Empty };

        /// Keep track of the registered fields so we don't register them twice
        /// I couldn't figure out where to register this in <see cref="NetLobbyScreen"/> where it wouldn't register twice
        /// It's probably not the most optimal way of doing this so feel free to change this
        /// <seealso cref="NetLobbyScreen.Select"/> where I'm utilizing this
        private readonly List<GUITextBox> registers = new List<GUITextBox>();

        // Selector index
        private int index;

        // Local changes we've made into previously stored messages
        private string[] localChanges;

        public ChatManager(bool loop, short maxCount)
        {
            this.loop = loop;
            this.maxCount = maxCount;
            localChanges = new string[maxCount];
        }

        public ChatManager()
        {
            localChanges = new string[maxCount];
        }


        /// <summary>
        /// Registers special input actions to the selected input field
        /// </summary>
        /// <param name="element">GUI Element we want to register</param>
        /// <param name="manager">Instance</param>
        public static void RegisterKeys(GUITextBox element, ChatManager manager)
        {
            // If already registered then don't register it again
            if (manager.registers.Any(p => element == p)) { return; }
            element.OnKeyHit += (sender, key) =>
            {
                switch (key)
                {
                    // Up/Down Arrow key history action
                    case Keys.Up:
                    case Keys.Down:
                    {
                        // Up arrow key? go up. Down arrow key? go down. Everything else gets binned
                        Direction direction = key == Keys.Up ? Direction.Up : (key == Keys.Down ? Direction.Down : Direction.Other);

                        string newMessage = manager.SelectMessage(direction, element.Text);
                        // Don't do anything if we didn't find anything
                        if (newMessage == null) { return; }

                        element.Text = newMessage;
                        break;
                    }
                    case Keys.Tab:
                        // TODO tab completion behavior, maybe?
                        break;
                }
            };
            manager.registers.Add(element);
        }

        // Store a new message
        public void Store(string message)
        {
            Clear();
            var strip = StripMessage(message);
            if (string.IsNullOrWhiteSpace(strip)) { return; }

            if (messageList.Count > 1 && messageList[1] == message) { return; }
            
            // insert to the second position as the first position is reserved for the original message if any
            messageList.Insert(1, message);
            // we don't want to add too many messages
            if (messageList.Count > maxCount)
            {
                messageList.RemoveAt(messageList.Count - 1);
            }

            // [It's also possible to lambdas too in short methods, if you like: string StripMessage(string text) => ChatMessage.GetChatMessageCommand(text, out string msg);]
            static string StripMessage(string text)
            {
                ChatMessage.GetChatMessageCommand(text, out string msg);
                return msg;
            }
        }

        /// <summary>
        /// Call this function whenever we should stop doing special stuff and return normal behavior.
        /// For example when you deselect the chat box.
        /// </summary>
        public void Clear()
        {
            index = 0;
            localChanges = new string[maxCount];
        }

        /// <summary>
        /// Scroll up or down on the message history and return a message
        /// </summary>
        /// <param name="direction">Direction we want to scroll the stack</param>
        /// <param name="original">Leftover text that is in the chat box when we override it</param>
        /// <returns>A message or null</returns>
        private string SelectMessage(Direction direction, string original)
        {
            var originalIndex = index;
            while (true)
            {
                if (direction == Direction.Other)
                {
                    return null;
                }

                // temporarily save our changes in case we fat-finger and want to go back
                localChanges[index] = original;

                var dir = (int) direction;

                var nextIndex = (index + dir);

                if (loop && messageList.Count > 1)
                {
                    nextIndex = LoopAround(nextIndex);
                }
                else
                {
                    if (nextIndex > messageList.Count - 1)
                    {
                        return null;
                    }
                }

                if (nextIndex >= 0 && EntryAt(nextIndex) == original && nextIndex != originalIndex && originalIndex != 0)
                {
                    index = nextIndex;
                    continue;
                }

                return nextIndex < 0 ? localChanges.FirstOrDefault() : EntryAt(index = nextIndex);

                string EntryAt(int i)
                {
                    // if we've previously edited the entry then give us that, else give us the original message
                    return localChanges[i] ?? messageList[i];
                }

                int LoopAround(int next)
                {
                    if (next > (messageList.Count - 1))
                    {
                        return 1;
                    }

                    if (next < 1)
                    {
                        return messageList.Count - 1;
                    }

                    return next;
                }
            }
        }

        // Used for Up/Down arrow key action
        private enum Direction
        {
            Up = 1,
            Down = -1,
            Other = 0
        }
    }
}