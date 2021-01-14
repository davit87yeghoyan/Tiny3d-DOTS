using Runtime.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Input;

namespace Runtime.Systems
{
    public class InputCollectorSystem : SystemBase
    {
        private float2? _oldPos;
        protected override void OnUpdate()
        {
            //Get the keyboard input data
            var inputAxis = new float2(0, 0);
            var input = World.GetOrCreateSystem<InputSystem>();
           
            if (input.GetKey(KeyCode.A) || input.GetKey(KeyCode.LeftArrow))
                inputAxis.x = -1;
            if (input.GetKey(KeyCode.D) || input.GetKey(KeyCode.RightArrow))
                inputAxis.x = 1;

            bool isTouch = false;
            if (input.IsTouchSupported())
            {
                //Get the touch input data
                Touch touch = input.GetTouch(0);
                isTouch = input.TouchCount() > 0 && touch.phase == TouchState.Moved;
                if (isTouch)
                {
                    var pos = CameraUtil.ScreenPointToWorldPoint(World, InputUtil.GetInputPosition(input));
                    if (_oldPos != null)
                    {
                        inputAxis =  (pos- (float2)_oldPos)* (touch.deltaX < 0 ? 1 : -1);
                    }
                    _oldPos = pos;
                }
                else
                {
                    _oldPos = null;
                } 
            }
           
            
            Entities.ForEach((ref PlayerInputComponent playerInput) =>
            {
                playerInput.InputAxis = inputAxis;
                playerInput.IsTouch = isTouch;
            }).WithBurst().Run();
        }
    }
}
