﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BananaMpq.Geometry;
using BananaMpq.Geometry.Builders;
using BananaMpq.Layer.WmoRelated;
using BananaMpq.View.Infrastructure;
using CrystalMpq.DataFormats;
using CrystalMpq.WoW;
using SharpDX;

namespace BananaMpq.Dumper
{
    delegate void SceneDumper(Scene scene, Stream s);

    class Program
    {
        private static readonly MpqFileReader FileReader = new MpqFileReader(WoWInstallation.Find()); // infers installation location from registry
        //private static readonly MpqFileReader FileReader = new MpqFileReader(WoWInstallation.AssumeAt("Z:\\World of Warcraft"));
        private static readonly FilePool Files = new FilePool(FileReader);
        private static readonly AdtGeometryBuilder Builder = new AdtGeometryBuilder(Files, MapLiquidType);
        private static readonly KeyedClientDatabase<int, LiquidTypeRecord> LiquidTypeDatabase = InitializeLiquidTypeDb();
        private static readonly SceneDumper Dumper = DumpAsWavefrontObject;

        private static KeyedClientDatabase<int, LiquidTypeRecord> InitializeLiquidTypeDb()
        {
            using (var stream = FileReader.Open("DBFilesClient\\LiquidType.dbc"))
            {
                return new KeyedClientDatabase<int, LiquidTypeRecord>(stream);
            }
        }

        private static LiquidTypeClass MapLiquidType(int liquidType)
        {
            var record = LiquidTypeDatabase[liquidType];
            return (LiquidTypeClass)record.TypeClass;
        }

        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Usage();
                return;
            }

            WowContinent cont;
            var continent = args[0];
            if (!Enum.TryParse(continent, out cont))
            {
                Console.Error.WriteLine("Continent '" + continent + "' not supported. Please try another or add the respective enum member.");
                return;
            }
            var x = int.Parse(args[1]);
            var y = int.Parse(args[2]);
            var scene = Builder.BuildTile(MpqFilePaths.MapToInternalName(cont), x, y);

            Dumper(scene, Console.OpenStandardOutput(65536));
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: BananaMpq.Dumper.exe continent x y");
            Console.WriteLine("       This will dump the whole adt to stdout. Pipe/redirect as you want to.");
            Console.WriteLine("       Example: BananaMpq.Dumper.exe EasternKingdoms 30 48 > \"EK(30, 48).obj\"");
        }

        private static void DumpAsWavefrontObject(Scene scene, Stream stream)
        {
            var allObjects = scene.Terrain
                .Concat(scene.Liquids)
                .Concat(scene.Wmos)
                .Concat(scene.Doodads);

            // Wavefront objects have exactly one vertex array and one triangle array, which indexes into the vertex array 1-based.
            // Since Vertices and especially vertex indices are stored per SceneObject, we have to copy the vertices and offset
            // the triangle indices:
            var vertices = new List<Vector3>();
            var triangles = new List<IndexedTriangle>();
            foreach (var geo in allObjects.Select(so => so.Geometry))
            {
                var offset = 1 + vertices.Count; // 1-based
                vertices.AddRange(geo.Vertices);
                triangles.AddRange(
                    from t in geo.Triangles
                    select new IndexedTriangle
                    {
                        A = t.A + offset,
                        B = t.B + offset,
                        C = t.C + offset
                    });
            }

            // Now just dump them.
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("o some_adt.obj");
                foreach (var v in vertices)
                {
                    var nf = CultureInfo.InvariantCulture.NumberFormat;
                    writer.WriteLine("v {0} {1} {2}", v.X.ToString(nf), v.Y.ToString(nf), v.Z.ToString(nf));
                }

                foreach (var t in triangles)
                {
                    writer.WriteLine("f {0} {1} {2}", t.A, t.B, t.C);
                }
            }
        }
    }
}
