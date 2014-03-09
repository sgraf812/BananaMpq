using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BananaMpq.View.Rendering;
using SharpDX;
using SharpDX.Direct3D9;

namespace BananaMpq.View.Infrastructure
{
    public static class PluginLoader
    {
        public static bool PluginExists { get { return NavMeshPlugin != null; } }
        public static INavMeshPlugin NavMeshPlugin { get; private set; }

        static PluginLoader()
        {
            try
            {
                // seriously. don't ever do this. my eyes bleed, but I'm to lazy to do this the right way
                var asm = Assembly.LoadFrom("BananaMpq.View.NavMesh.dll");
                var pluginType = asm.GetExportedTypes().First(typeof(INavMeshPlugin).IsAssignableFrom);
                // assume a parameterless constructor:
                NavMeshPlugin = (INavMeshPlugin)Activator.CreateInstance(pluginType);
            }
            catch
            {
                Console.Error.WriteLine("No plugin was found. Falling back to non NavMesh stuff");
            }
        }
    }

    public interface INavMeshPlugin
    {
        IEnumerable<INavigationMeshRenderer> CreateRenderers(Device device);
        void Reset();
        object BuildNavMesh(BuildConfiguration config);
        Vector3[] FindPath(Vector3 from, Vector3 to);
        Vector3? FindNearestPositionOnMesh(Vector3 pos);
    }
}