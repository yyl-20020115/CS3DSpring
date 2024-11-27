using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace CSWall;

/// <summary>
/// Index.xaml 的交互逻辑
/// </summary>
public partial class Index : Page
{
    private ParticleSystem system;
    private DispatcherTimer _frameTimer;
    private System.Windows.Point pMouse = new (-1, -1);
    private PerspectiveCamera Camera;

    public Index()
    {
        InitializeComponent();

        _frameTimer = new DispatcherTimer();
        _frameTimer.Tick += OnFrame;
        _frameTimer.Interval = TimeSpan.FromMilliseconds(100);
        _frameTimer.Start();

        system = new ParticleSystem(Colors.Blue);



        WorldModels.Children.Add(system.ParticleModel);

        this.Camera = new PerspectiveCamera
        {
            FieldOfView = 60
        };

    }

    private void OnFrame(object sender, EventArgs e)
    {
        system.UpdateGeometry();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        //避免页面关闭后资源占用
        _frameTimer.Stop();
        _frameTimer.Tick -= OnFrame;
        _frameTimer = null;
        system = null;
    }

    private void Grid_Drop(object sender, DragEventArgs e)
    {
        var f = e.Data.GetData(DataFormats.FileDrop);
        if(f is string[] files && files.Length == 1) {
            var bitmap =
                Bitmap.FromFile(files[0]) as Bitmap;
            if(bitmap != null)
            {
                system.SpawnParticle(bitmap);
            }
        }
    }
}
