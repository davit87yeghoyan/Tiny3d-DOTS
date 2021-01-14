using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Tiny.Input;
using Unity.Tiny.Rendering;

namespace Runtime.Systems
{
    /// <summary>
    ///     Translate mouse and touch inputs into overrideable functions
    ///     Create a convenience function to detect object under pointer
    /// </summary>
    public abstract class PointerSystemBase : SystemBase
    {
        protected InputSystem InputSystem;
        protected ScreenToWorld ScreenToWorld;

        protected readonly float m_RayDistance = 10000f;

        protected override void OnCreate()
        {
            InputSystem = World.GetExistingSystem<InputSystem>();
            ScreenToWorld = World.GetExistingSystem<ScreenToWorld>();
        }

        protected override void OnUpdate()
        {
            // Touch
            if (InputSystem.IsTouchSupported() && InputSystem.TouchCount() > 0)
            {
                for (var i = 0; i < InputSystem.TouchCount(); i++)
                {
                    var touch = InputSystem.GetTouch(i);
                    var inputPos = new float2(touch.x, touch.y);

                    switch (touch.phase)
                    {
                        case TouchState.Began:
                            OnInputDown(i, inputPos);
                            break;

                        case TouchState.Moved:
                            var inputDelta = new float2(touch.deltaX, touch.deltaY);
                            OnInputMove(i, inputPos, inputDelta);
                            break;

                        case TouchState.Stationary:
                            break;

                        case TouchState.Ended:
                            OnInputUp(i, inputPos);
                            break;

                        case TouchState.Canceled:
                            OnInputCanceled(i);
                            break;
                        default:
                            return;
                    }
                }

                return;
            }

            // Mouse
            if (InputSystem.IsMousePresent())
            {
                var inputPos = InputSystem.GetInputPosition();

                if (InputSystem.GetMouseButtonDown(0))
                {
                    OnInputDown(-1, inputPos);
                }
                else if (InputSystem.GetMouseButton(0))
                {
                    OnInputMove(-1, inputPos, InputSystem.GetInputDelta());
                }
                else if (InputSystem.GetMouseButtonUp(0))
                {
                    OnInputUp(-1, inputPos);
                }
            }
        }

        protected virtual void OnInputDown(int pointerId, float2 inputPos){}

        protected virtual void OnInputMove(int pointerId, float2 inputPos, float2 inputDelta){}

        protected virtual void OnInputUp(int pointerId, float2 inputPos){}

        protected virtual void OnInputCanceled(int pointerId) {}

        protected RaycastHit GetPointerRaycastHit(float2 inputPos)
        {
            ref PhysicsWorld physicsWorld = ref World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

            // Convert input position to ray going from screen to world
            ScreenToWorld.InputPosToWorldSpaceRay(inputPos, out var rayOrigin, out var rayDirection);

            var raycastInput = new RaycastInput
            {
                Start = rayOrigin,
                End = rayOrigin + rayDirection * m_RayDistance,
                Filter = CollisionFilter.Default
            };

            // Return top-most entity that was hit by ray
            physicsWorld.CastRay(raycastInput, out RaycastHit hit);
            return hit;
        }
    }
}