using System.Xml.Serialization;
using PrisonCopter.OptionsFramework.Attibutes;
using PrisonCopter.Attributes;

namespace PrisonCopter
{
    [Options("PrisonCopter-Options")]
    public class Options
    {       
        [HideWhenNotInAssetEditorCondition]
        [XmlIgnore]
        [Button("To prison helicopter", null, 
            nameof(PrisonCopterEditedAssetTransformer), nameof(PrisonCopterEditedAssetTransformer.ToPrisonCopter))]
        public object ToPrisonHelicopterButton { get; set; } = null;
    }
}

