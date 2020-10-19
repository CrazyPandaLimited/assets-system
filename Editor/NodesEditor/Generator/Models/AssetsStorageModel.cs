using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.AssetsSystem.CodeGen
{
    [ Serializable ]
    public sealed class AssetsStorageModel : ProcessorLinkInformation
    {
        private readonly HashSet< ProcessorModel > _processors = new HashSet< ProcessorModel >();
        private readonly HashSet< CtorParameter > _ctorParameters = new HashSet< CtorParameter >();

        private readonly NameResolver _localParametersNameResolver = new NameResolver();
        private readonly NameResolver _ctorParametersNameResolver = new NameResolver();

        public string DllName { get; set; } = string.Empty;
        public string PathToFinalDll { get; set; } = string.Empty;
        public string DllVersion { get; set; } = string.Empty;
        public string NameSpace { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public Type BaseType { get; set; } = default;
        public IEnumerable< CtorParameter > CtorParameters => _ctorParameters;
        public IEnumerable< ProcessorModel > Processors => _processors;

        public void AddCtorParameters(params CtorParameter[] ctorParameters )
        {
            foreach( var ctorParameter in ctorParameters )
            {
                ctorParameter.Name = _ctorParametersNameResolver.GetFixedName( ctorParameter.Name );
                _ctorParameters.Add( ctorParameter );
            }
        }
        
        public void AddProcessors( params ProcessorModel[] processors )
        {
            foreach( var processor in processors )
            {
                processor.Name = _localParametersNameResolver.GetFixedName( processor.Name );
                _processors.Add( processor );
            }
        }
    }
}