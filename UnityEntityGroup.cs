using System.Collections.Generic;
using System.Linq;
using System;
using Sceelix.Actors.Data;
using Sceelix.Core.Annotations;
using Sceelix.Core.Data;
using Sceelix.Core.IO;
using Sceelix.Core.Parameters;
using Sceelix.Core.Procedures;
using Sceelix.Mathematics.Data;
using Newtonsoft.Json;
using Sceelix.Unity.Annotations;

namespace Sceelix.MyNewEngineLibrary
{
    /// <summary>
    /// A Unity Entity Group. Extends an actor group with unity specific attributes.
    /// </summary>
    [Entity("Unity Entity Group")]
    public class UnityEntityGroup : ActorGroup, IUnityEntityGroup
    {
        private bool _enabled = true;
        private string _layer;
        private string _name = "Unity Entity Group";
        private string _prefab;
        private Vector3D _relativeScale = new Vector3D(1f, 1f, 1f);
        private bool _static;
        private string _tag;

        public string Name
        {
            get => this._name;
            set => this._name = value;
        }

        public string Tag
        {
            get => this._tag;
            set => this._tag = value;
        }

        public string Prefab
        {
            get => this._prefab;
            set => this._prefab = value;
        }

        public Vector3D RelativeScale
        {
            get => this._relativeScale;
            set => this._relativeScale = value;
        }

        public string ScaleMode { get; set; }

        public string Layer
        {
            get => this._layer;
            set => this._layer = value;
        }

        public bool Static
        {
            get => this._static;
            set => this._static = value;
        }

        public bool Enabled
        {
            get => this._enabled;
            set => this._enabled = value;
        }

        public Vector3D Scale => (this._relativeScale * this.BoxScope.Sizes).ReplaceValue(0.0f, 1f);

        public string Positioning { get; set; }
  

        public UnityEntityGroup(IEnumerable<IActor> actors) : base(actors)
        {
        }



        /// <summary>
        /// This function should be implemented.
        /// </summary>
        /// <returns></returns>
        public override IEntity DeepClone()
        {
            //this function performs a MemberwiseClone
            //at this point, all the attributes are cloned
            UnityEntityGroup clone = (UnityEntityGroup)base.DeepClone();

            clone._enabled = _enabled;
            clone._layer = _layer;
            clone._name = _name;
            clone._prefab = _prefab;
            clone._relativeScale = new Vector3D(_relativeScale);
            clone._static = _static;
            clone._tag = _tag;

            return clone;
        }
    }


    //This is an optional approach. You can define interfaces that
    //inherit from IEntity and use it generically in inputs/outputs.
    [Entity("Unity Group Thing")]
    interface IUnityEntityGroup : IEntityGroup
    {
        string Name { get; set; }
        bool Enabled { get; set; }
        bool Static { get; set; }
        string Tag { get; set; }
        string Layer { get; set; }

    }

    /// <summary>
    /// Similar to "Actor Group" but decorates the group node with meaningful parameters for Unity.
    /// </summary>
    [Procedure("0DABC5F2-C6EE-40E6-9576-14F9D48BAA84", Label = "Unity Entity Group Create")]
    public class UnityEntityGroupCreateProcedure : SystemProcedure
    {
        /// <summary>Set of actors to be grouped.</summary>
        private readonly CollectiveInput<IActor> _input = new CollectiveInput<IActor>("Actors");
        /// <summary>Actor group with Unity Entity parameters</summary>
        private readonly Output<UnityEntityGroup> _output = new Output<UnityEntityGroup>("Unity Entity Group");

        /// <summary>
        /// Name of the Game Object, as it appears in the "Hierarchy" panel.
        /// </summary>
        private readonly StringParameter _parameterName = new StringParameter("Name", "Group");

        /// <summary>
        /// Enabled flag that allows gameobject state to be toggled.
        /// </summary>
        private readonly BoolParameter _parameterEnabled = new BoolParameter("Enabled", true);

        /// <summary>
        /// IsStatic flag that allows gameobject drawing to be optimized.
        /// </summary>
        private readonly BoolParameter _parameterIsStatic = new BoolParameter("Static", true);

        /// <summary>Layer of the Game Object.</summary>
        private readonly StringParameter _parameterLayer = new StringParameter("Layer", "");

        /// <summary>
        /// Path to the prefab, relative to the "Assets" folder.e.g.
        /// For example Assets/MyFolder/myprefab
        /// </summary>
        private readonly StringParameter _parameterPrefab = new StringParameter("Prefab", "");

        /// <summary>
        /// Defines how the prefab positioning should be placed within the Game Object scope.<br />
        /// <b>Minimum</b> means that prefab will be translated so that its minimum point will match the scope minimum.
        /// <b>Pivot</b> means that the prefab will be translated so that its pivot point will match the scope minimum.<br />
        /// </summary>
        private readonly ChoiceParameter _parameterPositioning = new ChoiceParameter("Positioning", "Minimum", new string[2]
        {
          "Minimum",
          "Pivot"
        });

        /// <summary>
        /// Defines how the prefab dimensions should be placed within the Game Object scope.<br />
        /// <b>None</b> means that prefabs will be left with its original scaling.
        /// <b>Stretch To Fill</b> means that the prefab will stretched to fill the scope volume.<br />
        /// <b>Scale To Fit</b> means that the prefab will be scaled, maintaining aspect ratio, so it completely fits withing the scope volume.
        /// </summary>
        private readonly ChoiceParameter _parameterScaleMode = new ChoiceParameter("Scale Mode", "Stretch To Fill", new string[3]
        {
          "None",
          "Stretch To Fill",
          "Scale To Fit"
        });
        /// <summary>Tag of the Game Object.</summary>
        private readonly StringParameter _parameterTag = new StringParameter("Tag", "");

        protected override void Run()
        {
            IEnumerable<IActor> actors = this._input.Read();

            UnityEntityGroup ueg = new UnityEntityGroup(actors)
            {
                Name = this._parameterName.Value,
                Tag = this._parameterTag.Value,
                Layer = this._parameterLayer.Value,
                Prefab = this._parameterPrefab.Value,
                ScaleMode = this._parameterScaleMode.Value,
                Static = this._parameterIsStatic.Value,
                Enabled = this._parameterEnabled.Value,
                Positioning = this._parameterPositioning.Value
            };

            this._output.Write(ueg);

        }

    }

    [UnityJsonConverter(typeof(IUnityEntityGroup))]
    public class EntityGroupConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IUnityEntityGroup entityGroup = (IUnityEntityGroup)value;

            writer.WriteStartObject();
            writer.WritePropertyName("EntityType");
            writer.WriteValue("EntityGroup");
            writer.WritePropertyName("GroupType");
            writer.WriteValue(entityGroup.GetType().Name);
            writer.WritePropertyName("Name");
            writer.WriteValue(entityGroup.Name);
            writer.WritePropertyName("Static");
            writer.WriteValue(entityGroup.Static);
            writer.WritePropertyName("Enabled");
            writer.WriteValue(entityGroup.Enabled);
            writer.WritePropertyName("Tag");
            writer.WriteValue(entityGroup.Tag);
            writer.WritePropertyName("Layer");
            writer.WriteValue(entityGroup.Layer);

            writer.WritePropertyName("SubEntities");
            writer.WriteStartArray();

            foreach(IEntity current in entityGroup.SubEntities)
                serializer.Serialize(writer, current);

            writer.WriteEndArray();

            writer.WriteEndObject();
        }

        public override object ReadJson(
          JsonReader reader,
          Type objectType,
          object existingValue,
          JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType) => throw new NotImplementedException();
    }        
}
