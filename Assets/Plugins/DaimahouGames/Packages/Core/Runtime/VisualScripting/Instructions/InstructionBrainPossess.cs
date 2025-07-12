using System;
using System.Threading.Tasks;
using DaimahouGames.Runtime.Core;
using DaimahouGames.Runtime.Pawns;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Runtime.VisualScripting
{
    [Version(0, 1, 1)]

    [Description("Activate controls for the character, requires a pawn with a brain feature")]

    [Category("Pawn/Possess character")]

    [Parameter("Character", "The character target")]
    [Parameter("DisableCharacterControls", "If on, disable character controls")]
    [Image(typeof(IconBrain), ColorTheme.Type.Green)]

    [Keywords("Possess")]
    
    [Serializable]
    public class InstructionBrainPossess : Instruction
    {
        //============================================================================================================||
        // ※  Variables: --------------------------------------------------------------------------------------------|
        // ---| Exposed State -------------------------------------------------------------------------------------->|

        [SerializeField] 
        private PropertyGetGameObject m_Character = GetGameObjectPlayer.Create();
        [SerializeField] 
        private PropertyGetBool m_DisableCharacterControls;
        
        // ---| Internal State ------------------------------------------------------------------------------------->|
        // ---| Dependencies --------------------------------------------------------------------------------------->|
        // ---| Properties ----------------------------------------------------------------------------------------->|

        public override string Title => "Possess Character";

        // ---| Events --------------------------------------------------------------------------------------------->|
        // ※  Initialization Methods: -------------------------------------------------------------------------------|
        // ※  Public Methods: ---------------------------------------------------------------------------------------|
        // ※  Virtual Methods: --------------------------------------------------------------------------------------|
        
        protected override Task Run(Args args)
        {
            var pawn = m_Character.Get<Pawn>(args);
            var brain = pawn != null ? pawn.GetFeature<Brain>() : null;

            if (brain != null && brain.Controller != null)
            {
                brain.Controller.Possess(pawn);    
            }
            
            return Task.CompletedTask;
        }
        
        // ※  Private Methods: --------------------------------------------------------------------------------------|
        //============================================================================================================||
    }
}