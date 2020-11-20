using RogueElements;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev.ViewModels
{
    public class GroundTabEntitiesViewModel : ViewModelBase
    {
        public GroundTabEntitiesViewModel()
        {

        }

        public void ProcessInput(InputManager input)
        {
            //Loc groundCoords = GroundEditScene.Instance.ScreenCoordsToGroundCoords(input.MouseLoc);
            //switch (EntMode)
            //{
            //    case EntEditMode.PlaceEntity:
            //        {
            //            if (input.JustReleased(FrameInput.InputType.LeftMouse))
            //                PlaceEntity(groundCoords);
            //            else if (input.JustReleased(FrameInput.InputType.RightMouse))
            //                RemoveEntityAt(groundCoords);
            //            break;
            //        }
            //    case EntEditMode.SelectEntity:
            //        {
            //            if (input.JustReleased(FrameInput.InputType.LeftMouse))
            //                SelectEntityAt(groundCoords);
            //            else if (input.JustReleased(FrameInput.InputType.RightMouse))
            //                EntityContext(input.MouseLoc, groundCoords);
            //            break;
            //        }
            //    case EntEditMode.MoveEntity:
            //        {
            //            if (input.JustReleased(FrameInput.InputType.LeftMouse))
            //                MoveEntity(groundCoords);
            //            break;
            //        }
            //}
        }
    }
}
