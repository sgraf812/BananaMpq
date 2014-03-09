using System;
using System.Windows;
using System.Windows.Input;
using SharpDX;

namespace BananaMpq.View.Infrastructure
{
    public class CameraChassis
    {
        private const float Pi = (float)Math.PI;
        private bool _w, _a, _s, _d, _x, _space, _shift;
        private float _acceleration;
        private Vector3 _velocity;

        public float Pitch { get; set; }
        public float Yaw { get; set; }
        public Vector3 MovementDelta
        {
            get { return _velocity * (_shift ? 5.0f : 1.0f); }
        }

        public float Inertia
        {
            get { return 1.0f/_acceleration - 1.0f; }
            set { _acceleration = 1.0f/(1.0f + value); }
        }

        public CameraChassis()
        {
            Inertia = 0.0f;
        }

        public void HandleInputDelta(Vector delta)
        {
            Yaw += (float)delta.X / 100.0f;
            Yaw += Yaw > 2*Pi ? -2*Pi : Yaw < 0.0f ? 2*Pi : 0.0f;

            Pitch += (float)delta.Y / 100.0f;
            Pitch = Clamp(Pitch, -Pi/2, Pi/2);
        }

        public void HandleKey(KeyEventArgs args)
        {
            if (args.IsRepeat) return;
            switch (args.Key)
            {
                case Key.W:
                    _w = args.IsDown;
                    break;
                case Key.A:
                    _a = args.IsDown;
                    break;
                case Key.S:
                    _s = args.IsDown;
                    break;
                case Key.D:
                    _d = args.IsDown;
                    break;
                case Key.X:
                    _x = args.IsDown;
                    break;
                case Key.Space:
                    _space = args.IsDown;
                    break;
                case Key.LeftShift:
                    _shift = args.IsDown;
                    break;
            }
        }

        public void Sample()
        {
            _velocity *= 1.0f - _acceleration;
            _velocity = new Vector3(
                Clamp(_velocity.X + Accelerator(_w) - Accelerator(_s), -1.0f, 1.0f),
                Clamp(_velocity.Y + Accelerator(_d) - Accelerator(_a), -1.0f, 1.0f),
                Clamp(_velocity.Z + Accelerator(_space) - Accelerator(_x), -1.0f, 1.0f));
        }

        private float Accelerator(bool pressed)
        {
            return _acceleration*(pressed ? 1.0f : -1.0f);
        }

        private static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }
}