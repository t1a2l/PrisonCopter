using System.Xml.Serialization;
using PrisonHelicopter.Attibutes;
using PrisonHelicopter.OptionsFramework.Attibutes;


namespace PrisonHelicopter
{
    [Options("PrisonHelicopter-Options")]
    public class Options
    {
        private const string SETTINGS_UI = "SETTINGS_UI";

        [HideWhenNotInAssetEditorCondition]
        [XmlIgnore]
        [Button("To prison helicopter", null, 
            nameof(PrisonHelicopterEditedAssetTransformer), nameof(PrisonHelicopterEditedAssetTransformer.ToPrisonHelicopter))]
        public object ToPrisonHelicopterButton { get; set; } = null;


        [HideWhenNotInGameCondition]
        [DropDown("SETTINGS_VEHICLE_EDITOR_POSITION", nameof(percentNum), SETTINGS_UI)]
        public int priosnersPercentage { get; set; } = (int)percentNum.Ninty;
    }
}

