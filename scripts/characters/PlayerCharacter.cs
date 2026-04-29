using Godot;
using GodotUtilities;

namespace NeuralZeroProtocol.Scripts.Characters
{
    [GlobalClass]
    [Scene]
    public partial class PlayerCharacter : Character
    {
        [Node] public ActionValidatorComponent ActionValidatorComponent;

        public override void _Notification(int what)
        {
            if (what == NotificationSceneInstantiated)
			{
				WireNodes();
			}
        }
    }
}

