# dotNet5786_4652_2851

## Delivery System - .NET 8 WPF Application

A comprehensive delivery management system built with .NET 8, featuring a three-layer architecture (DAL, BL, PL), multithreaded simulator, and modern WPF user interface.

---

## ?? Implemented Bonuses Summary

| Category | Bonus Description | Points | Status | Implementation Details |
|----------|-------------------|--------|--------|------------------------|
| **Development Environment** |||| |
| | Using C# | - | ? Implemented | Entire project is written in C# 12.0 |
| | Full and correct use of TryParse in BlTest/DalTest | 1 | ? Implemented | `DalTest\Program.cs` - All input parsing uses TryParse with return value checking and inline variable declaration (e.g., `int.TryParse()`, `double.TryParse()`, `DateTime.TryParse()`, `Enum.TryParse()`) |
| **DAL Layer** |||| |
| | Password property added | 2 | ? Implemented | `DO\Courier.cs` contains `CourierPassword` property, stored and managed in both DalList and DalXml |
| | DalList & DalXml as Thread-Safe Lazy Singleton | 2 | ? Implemented | Both `DalList.cs` and `DalXml.cs` use `Lazy<T>` with `LazyThreadSafetyMode.PublicationOnly` pattern for thread-safe lazy initialization |
| **BL Layer** |||| |
| | Initial password provided by Admin, Courier can update | 1 | ? Implemented | `Initialization.cs` creates couriers with initial passwords; `CourierImplementation.UpdateCourier()` allows courier to update their own password |
| | Courier can choose distance type (Air/Walk/Car) | 3 | ? Implemented | `Tools.GetActualDistanceAsync()` calculates distance based on `VehicleType` using OSRM API with profiles: car, bike, foot |
| **PL Layer - Advanced WPF** |||| |
| | Interactive graphical display for invalid input | 1 | ? Implemented | `LoginWindow.xaml` shows/hides error TextBlocks (`ErrId`, `ErrPass`) with red foreground on validation failure |
| | Validation integrated with data binding | 1 | ? Implemented | `TextBoxes.xaml` - Styles use Triggers to change appearance based on `IsReadOnly`; Converters validate enum values |
| | Icon in window title and taskbar | 1 | ? Implemented | `PL\Helpers\Image.ico` used in `LoginWindow.xaml` as application icon |
| | Property Triggers | 1 | ? Implemented | `Buttons.xaml` - `IsMouseOver`, `IsPressed` triggers; `TextBoxes.xaml` - `IsReadOnly` trigger |
| | Data Triggers | 1 | ? Implemented | `DataGrids.xaml` - `EnumDataGridCellStyle` uses DataTrigger for enum-to-color conversion |
| | Use of ControlTemplate | 1 | ? Implemented | `Buttons.xaml` - Custom `ControlTemplate` for `BaseButtonStyle` with Border, ContentPresenter, and triggers |
| | ENTER key acts as button click | 1 | ? Implemented | `LoginWindow.xaml.cs` - `HandleEnterKey` method handles `Key.Enter` to trigger login or focus navigation |
| | Password displayed as asterisks (PasswordBox) | 1 | ? Implemented | `LoginWindow.xaml` uses `<PasswordBox x:Name="txtPassword">` which masks password input |
| | Delete button visible only if deletable (Courier) | 2 | ? Implemented | `Converters.cs` - `ConvertDeleteToEnabled` calls `IsCourierDeletable()` to control button visibility/enabled state |
| | Delete button visible only if cancellable (Order) | 2 | ? Implemented | `Converters.cs` - `ConvertCancelToEnabled` checks `OrderStatus` (Open/InProgress) to control cancel button |
| **Simulator** |||| |
| | BL throws exception during simulator, PL catches gracefully | - | ? Implemented | `AdminManager.ThrowOnSimulatorIsRunning()` throws `BlTemporaryNotAvailableException`; All PL button handlers have try-catch showing MessageBox |

---

## ?? Total Bonus Points

| Category | Points Achieved |
|----------|----------------|
| Development Environment | **1** |
| DAL Layer | **4** |
| BL Layer | **4** |
| PL Layer - Advanced WPF | **11** |
| **Total** | **20** |

---

## ??? Architecture Overview

### Three-Layer Architecture:
1. **DAL (Data Access Layer)** - XML and List-based data storage with Singleton pattern
2. **BL (Business Logic)** - Core business rules, validation, and simulator
3. **PL (Presentation Layer)** - WPF UI with MVVM-like patterns

### Key Features:
- **Multithreaded Simulator** - Automatic order assignment and delivery completion
- **Observer Pattern** - Real-time UI updates across all windows
- **Thread-Safe Operations** - Mutex-based protection for concurrent access
- **External API Integration** - Nominatim (geocoding) and OSRM (routing) APIs

---

## ?? Technical Highlights

### DAL Layer
- **Dual Implementation**: `DalList` (in-memory) and `DalXml` (file-based persistence)
- **Factory Pattern**: Dynamic DAL selection via configuration
- **Thread-Safe Singleton**: Using `Lazy<T>` for safe lazy initialization

### BL Layer
- **Observer Manager**: Notifies PL of data changes
- **Async Operations**: Distance calculations and geocoding
- **Simulator**: Background thread with automatic order processing

### PL Layer
- **Custom Controls**: `LabeledTextBox`, `LabeledDatePicker`, `TopBarControl`
- **Value Converters**: `EnumToColorConverter`, `InverseBoolConverter`, `SimulatorButtonTextConverter`
- **Resource Dictionaries**: Centralized styles for buttons, textboxes, datagrids, labels

---

## ?? Getting Started

1. Open `dotNet5786_4652_2851.sln` in Visual Studio 2022
2. Set `PL` as the startup project
3. Build and run the solution
4. Login credentials after initialization:
   - **Admin**: ID: `333333333`, Password: `1`

---

## ?? Project Structure

```
dotNet5786_4652_2851/
??? DalFacade/      # DAL interfaces and data objects (DO)
??? DallList/           # In-memory DAL implementation
??? DalXml/           # XML-based DAL implementation
??? DalTest/            # DAL testing console application
??? BL/ # Business Logic layer
??? BlTest/   # BL testing console application
??? PL/           # WPF Presentation Layer
?   ??? Controls/       # Custom user controls
? ??? Converters.cs   # Value converters
?   ??? Helpers/        # Utility classes
?   ??? Styles/ # XAML resource dictionaries
?   ??? Courier/        # Courier management windows
?   ??? Order/    # Order management windows
?   ??? Login/    # Authentication windows
?   ??? delivery/       # Delivery history windows
??? Stage0/   # Initial stage project
```

---

## ?? Authors

- Student ID: 4652
- Student ID: 2851