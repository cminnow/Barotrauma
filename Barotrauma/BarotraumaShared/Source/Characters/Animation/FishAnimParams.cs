﻿namespace Barotrauma
{
    abstract class FishAnimParams : AnimParams
    {
        protected FishAnimParams(string file) : base(file) { }

        public static FishAnimParams GetInstance(string speciesName)
        {
            switch (speciesName)
            {
                case "mantis":
                    return MantisAnimParams.Instance;
                case "carrier":
                    return CarrierAnimParams.Instance;
                default:
                    return null;
            }
        }

        [Serialize(1f, true), Editable]
        public float WaveAmplitude
        {
            get;
            set;
        }

        [Serialize(1f, true), Editable]
        public float WaveLength
        {
            get;
            set;
        }

        [Serialize(25.0f, true), Editable]
        public float SteerTorque
        {
            get;
            set;
        }
    }

    class CarrierAnimParams : FishAnimParams
    {
        public static CarrierAnimParams Instance = new CarrierAnimParams("Content/Characters/Carrier/CarrierAnimParams.xml");
        protected CarrierAnimParams(string file) : base(file) { }
    }

    class MantisAnimParams : FishAnimParams
    {
        public static MantisAnimParams Instance = new MantisAnimParams("Content/Characters/Mantis/MantisAnimParams.xml");
        protected MantisAnimParams(string file) : base(file) { }
    }
}
