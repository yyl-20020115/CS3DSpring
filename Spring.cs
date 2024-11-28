using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CS3DSpring;

public static class Spring
{
    public const double _2PI = 2.0 * Math.PI;

    public static ModelVisual3D BuildDonutModelVisual3D(
        double R = 50, int Segments = 360, double r = 10, Color? light = null, Brush? brush = null)
    {
        brush ??= Brushes.Green;
        var model = new ModelVisual3D() { Content = new Model3DGroup() };
        var group = model.Content as Model3DGroup;
        var collection = new Model3DCollection
            {
                new AmbientLight
                {
                    Color = light ?? Colors.White,
                }
            };


        var Positions = new Point3DCollection();
        var Indices = new Int32Collection();
        var Textures = new PointCollection();
        var geometry = new MeshGeometry3D();
        var material = new DiffuseMaterial(brush);

        double delta_angle = _2PI / Segments;
        double theta = 0.0;
        for (int i = 0; i < Segments; i++)
        {
            var phi = 0.0;
            for (int j = 0; j < Segments; j++)
            {
                Positions.Add(new
                    ((R + r * Math.Cos(phi)) * Math.Cos(theta),
                     (R + r * Math.Cos(phi)) * Math.Sin(theta),
                     r * Math.Sin(phi)
                    ));
                phi += delta_angle;
            }
            theta += delta_angle;
        }

        for (int current_layer = 0; current_layer < Segments; current_layer++)
        {
            int next_layer = (current_layer + 1) % Segments;

            for (int current_point = 0; current_point < Segments; current_point++)
            {
                int next_point = (current_point + 1) % Segments;
                int p0 = current_layer * Segments + current_point;
                int p1 = current_layer * Segments + next_point;
                int p2 = next_layer * Segments + current_point;
                int p3 = next_layer * Segments + next_point;

                Indices.Add(p1);
                Indices.Add(p0);
                Indices.Add(p2);

                Indices.Add(p2);
                Indices.Add(p3);
                Indices.Add(p1);

            }
        }

        geometry.Positions = Positions;
        geometry.TriangleIndices = Indices;
        geometry.TextureCoordinates = Textures;
        collection.Add(new GeometryModel3D(geometry, material));
        group!.Children = collection;
        return model;
    }

    public static ModelVisual3D BuildSpringModelVisual3D(
    int Coils = 4, double R = 50, int Segments = 360, double r = 10, Color? light = null, Brush? brush = null)
    {
        brush ??= Brushes.Green;
        var model = new ModelVisual3D() { Content = new Model3DGroup() };
        var group = model.Content as Model3DGroup;
        var collection = new Model3DCollection
            {
                new AmbientLight
                {
                    Color = light ?? Colors.White,
                }
            };


        var Positions = new Point3DCollection();
        var Indices = new Int32Collection();
        var Textures = new PointCollection();
        var geometry = new MeshGeometry3D();
        var material = new DiffuseMaterial(brush);

        double delta_angle = _2PI / Segments;
        double theta = 0.0;
        double delta_z = r * Coils / (Segments * Segments);
        double z = 0.0;

        for (int c = 0; c < Coils; c++)
        {
            for (int i = 0; i < Segments; i++)
            {
                var phi = 0.0;
                for (int j = 0; j < Segments; j++)
                {
                    Positions.Add(new
                        ((R + r * Math.Cos(phi)) * Math.Cos(theta),
                         (R + r * Math.Cos(phi)) * Math.Sin(theta),
                         z + r * Math.Sin(phi)
                        ));
                    z += delta_z;
                    phi += delta_angle;
                }
                theta += delta_angle;
            }
        }


        //必须减去1，避免闭合
        for (int current_layer = 0; current_layer < Segments * Coils - 1; current_layer++)
        {
            int next_layer = (current_layer + 1) % (Segments * Coils);

            for (int current_point = 0; current_point < Segments; current_point++)
            {
                int next_point = (current_point + 1) % Segments;
                int p0 = current_layer * Segments + current_point;
                int p1 = current_layer * Segments + next_point;
                int p2 = next_layer * Segments + current_point;
                int p3 = next_layer * Segments + next_point;

                Indices.Add(p1);
                Indices.Add(p0);
                Indices.Add(p2);

                Indices.Add(p2);
                Indices.Add(p3);
                Indices.Add(p1);

            }
        }

        Positions.Add(new Point3D(R, 0, 0));
        Positions.Add(new Point3D(R, 0, z));
        int first_center = Positions.Count - 2;
        int last_center = Positions.Count - 1;
        for (int current_point = 0; current_point < Segments - 1; current_point++)
        {
            int next_point = (current_point + 1) % Segments;

            Indices.Add(current_point);
            Indices.Add(next_point);
            Indices.Add(first_center);
        }
        for (int current_point = Positions.Count - 4; current_point >= Positions.Count - 3 - Segments; current_point--)
        {
            int next_point = current_point + 1;

            Indices.Add(next_point);
            Indices.Add(current_point);
            Indices.Add(last_center);
        }
        geometry.Positions = Positions;
        geometry.TriangleIndices = Indices;
        geometry.TextureCoordinates = Textures;
        collection.Add(new GeometryModel3D(geometry, material));
        group!.Children = collection;
        return model;
    }

    public static ModelVisual3D BuildDonutSpringModelVisual3D(
    double GR = 250, int Coils = 24,  double R = 50, int Segments = 360, double r = 10, Color? light = null, Brush? brush = null)
    {
        brush ??= Brushes.Green;
        var model = new ModelVisual3D() { Content = new Model3DGroup() };
        var group = model.Content as Model3DGroup;
        var collection = new Model3DCollection
            {
                new AmbientLight
                {
                    Color = light ?? Colors.White,
                }
            };


        var Positions = new Point3DCollection();
        var Indices = new Int32Collection();
        var Textures = new PointCollection();
        var geometry = new MeshGeometry3D();
        var material = new DiffuseMaterial(brush);

        var delta_angle = _2PI / Segments;
        var delta_yeta = _2PI/ Coils;
        var yeta = 0.0;

        for (int c = 0; c < Coils; c++)
        {
            var theta = 0.0;
            for (int i = 0; i < Segments; i++)
            {
                var phi = 0.0;
                for (int j = 0; j < Segments; j++)
                {
                    //TODO:
                    Positions.Add(new
                        ((GR + R * Math.Cos(theta)) * Math.Cos(yeta),
                         (GR + R * Math.Cos(theta)) * Math.Sin(yeta),
                         0 + r * Math.Sin(phi)
                        ));

                    phi += delta_angle;
                }
                theta += delta_angle;
            }
            yeta += delta_yeta;
        }
        //可以闭合
        for (int current_layer = 0; current_layer < Segments * Coils; current_layer++)
        {
            int next_layer = (current_layer + 1) % (Segments * Coils);

            for (int current_point = 0; current_point < Segments; current_point++)
            {
                int next_point = (current_point + 1) % Segments;
                int p0 = current_layer * Segments + current_point;
                int p1 = current_layer * Segments + next_point;
                int p2 = next_layer * Segments + current_point;
                int p3 = next_layer * Segments + next_point;

                Indices.Add(p1);
                Indices.Add(p0);
                Indices.Add(p2);

                Indices.Add(p2);
                Indices.Add(p3);
                Indices.Add(p1);
            }
        }
        geometry.Positions = Positions;
        geometry.TriangleIndices = Indices;
        geometry.TextureCoordinates = Textures;
        collection.Add(new GeometryModel3D(geometry, material));
        group!.Children = collection;
        return model;
    }
}
