using System.ComponentModel;
using System.Windows.Media;

namespace Clothesline;

/// <summary>绳子上的一张"晾着"的截图。</summary>
public sealed class PegItem : INotifyPropertyChanged
{
    public string FilePath { get; }
    public ImageSource Bitmap { get; }
    public double Angle { get; }

    public RotateTransform Rot { get; }
    public ScaleTransform Scale { get; }
    public TransformGroup Xf { get; }

    private bool _copyFeedback;
    public bool CopyFeedback
    {
        get => _copyFeedback;
        set
        {
            if (_copyFeedback == value) return;
            _copyFeedback = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CopyFeedback)));
        }
    }

    public PegItem(string filePath, ImageSource bitmap, double angle)
    {
        FilePath = filePath;
        Bitmap = bitmap;
        Angle = angle;
        Rot = new RotateTransform(angle);
        Scale = new ScaleTransform(1, 1);
        Xf = new TransformGroup();
        Xf.Children.Add(Rot);
        Xf.Children.Add(Scale);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
