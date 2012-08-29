using SharpDX;

namespace BananaMpq.View.Infrastructure
{
    public class FirstPersonCamera
    {
        private readonly CameraChassis _chassis;

        public CameraChassis Chassis { get { return _chassis; } }
        public Vector3 Position { get; set; }
        public float Velocity { get; set; }
        public Vector3 Forward { get; set; }
        public Vector3 Right { get; set; }
        public Vector3 Up { get; set; }
        public Matrix WorldView { get; private set; }

        public FirstPersonCamera(CameraChassis chassis)
        {
            _chassis = chassis;
            Velocity = 1.0f;
            Right = Vector3.UnitX;
            Forward = Vector3.UnitY;
            Up = Vector3.UnitZ;
        }

        public void Sample(float interpolation)
        {
            _chassis.Sample();
            var movementDelta = _chassis.MovementDelta * interpolation * Velocity;
            movementDelta = Forward * movementDelta.X + Right * movementDelta.Y + Up * movementDelta.Z;
            
            var rotation = Matrix.RotationAxis(Right, -_chassis.Pitch) * Matrix.RotationAxis(Up, -_chassis.Yaw);
            Position += (Vector3)Vector3.Transform(movementDelta, rotation);
            rotation.Transpose();

            // T^(-1) = (matRotation(phi) * matTranslation(p))^(-1) = matTranslation(-p) * matRotation(-phi) (right-to-left)
            WorldView = Matrix.Translation(-Position) * rotation * DeviceTransform;
        }

        private Matrix DeviceTransform
        {
            get
            {
                // T_C_E = camera coordinate transform, T_D_E = device coordinate transform
                // So: T_C_D needed: T_C_D = T_C_E * T_E_D
                // where E means the default rhw coordinate system, i.e. UnitX = Right, UnitY = Forward, UnitZ = Up
                var ted = new Matrix(
                    1, 0, 0, 0, 
                    0, 0, -1, 0, 
                    0, 1, 0, 0, 
                    0, 0, 0, 1);
                var tce = new Matrix
                {
                    Column1 = (Vector4)Right, 
                    Column2 = (Vector4)Forward, 
                    Column3 = (Vector4)Up,
                    M44 = 1.0f
                };
                return tce*ted;
            }
        }
    }
}