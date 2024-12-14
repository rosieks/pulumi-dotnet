// *** WARNING: this file was generated by pulumi-language-dotnet. ***
// *** Do not edit by hand unless you're certain you know what you are doing! ***

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Pulumi.Serialization;

namespace Pulumi.PrimitiveRef.Inputs
{

    public sealed class DataArgs : global::Pulumi.ResourceArgs
    {
        [Input("boolArray", required: true)]
        private InputList<bool>? _boolArray;
        public InputList<bool> BoolArray
        {
            get => _boolArray ?? (_boolArray = new InputList<bool>());
            set => _boolArray = value;
        }

        [Input("boolean", required: true)]
        public Input<bool> Boolean { get; set; } = null!;

        [Input("float", required: true)]
        public Input<double> Float { get; set; } = null!;

        [Input("integer", required: true)]
        public Input<int> Integer { get; set; } = null!;

        [Input("string", required: true)]
        public Input<string> String { get; set; } = null!;

        [Input("stringMap", required: true)]
        private InputMap<string>? _stringMap;
        public InputMap<string> StringMap
        {
            get => _stringMap ?? (_stringMap = new InputMap<string>());
            set => _stringMap = value;
        }

        public DataArgs()
        {
        }
        public static new DataArgs Empty => new DataArgs();
    }
}
