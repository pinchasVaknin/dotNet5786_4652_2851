using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PL.Order;

public class OrderItem : INotifyPropertyChanged
{
    private string _model = "";
    private int _quantity;
    private double _price;

    public string Model
    {
        get => _model;
        set { _model = value; OnPropertyChanged(); }
    }

    public int Quantity
    {
        get => _quantity;
        set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalPrice)); }
    }

    public double Price
    {
        get => _price;
        set { _price = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalPrice)); }
    }

    public double TotalPrice => Price * Quantity;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
