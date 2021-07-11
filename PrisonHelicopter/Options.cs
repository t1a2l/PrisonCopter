using System.Xml.Serialization;
using PrisonHelicopter.Attibutes;
using PrisonHelicopter.OptionsFramework.Attibutes;


namespace PrisonHelicopter
{
    [Options("PrisonHelicopter-Options")]
    public class Options
    {       
        [HideWhenNotInAssetEditorCondition]
        [XmlIgnore]
        [Button("To prison helicopter", null, 
            nameof(PrisonHelicopterEditedAssetTransformer), nameof(PrisonHelicopterEditedAssetTransformer.ToPrisonHelicopter))]
        public object ToPrisonHelicopterButton { get; set; } = null;
    }
}

