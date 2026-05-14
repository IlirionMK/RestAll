using RestAll.Desktop.Core.Reservations;
using RestAll.Desktop.App.Validation;
using FluentValidation.Results;

namespace RestAll.Desktop.App.ViewModels;

public class ReservationsViewModel : CancelableViewModelBase
{
    private readonly IManageReservationsUseCase _reservationsUseCase;
    private readonly ReservationValidator _validator;
    
    private List<Reservation> _reservations = new();
    private DateTime _selectedDate = DateTime.Today;
    private string _customerName = "";
    private string _customerPhone = "";
    private string _customerEmail = "";
    private DateTime _reservationDate = DateTime.Today;
    private DateTime _reservationTime = DateTime.Now;
    private string _tableId = "";
    private string _numberOfGuests = "2";
    private string _specialRequests = "";
    private string _cancelReservationId = "";

    public ReservationsViewModel(IManageReservationsUseCase reservationsUseCase)
    {
        _reservationsUseCase = reservationsUseCase;
        _validator = new ReservationValidator();
        
        LoadReservationsCommand = new AsyncRelayCommand(LoadReservationsAsync, () => !IsLoading);
        CreateReservationCommand = new AsyncRelayCommand(CreateReservationAsync, () => !IsLoading && IsValidReservationData());
        CancelReservationCommand = new AsyncRelayCommand(CancelReservationAsync, () => !IsLoading && !string.IsNullOrWhiteSpace(CancelReservationId));
    }

    public List<Reservation> Reservations
    {
        get => _reservations;
        set => SetProperty(ref _reservations, value);
    }

    public DateTime SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }

    public string CustomerName
    {
        get => _customerName;
        set => SetProperty(ref _customerName, value);
    }

    public string CustomerPhone
    {
        get => _customerPhone;
        set => SetProperty(ref _customerPhone, value);
    }

    public string CustomerEmail
    {
        get => _customerEmail;
        set => SetProperty(ref _customerEmail, value);
    }

    public DateTime ReservationDate
    {
        get => _reservationDate;
        set => SetProperty(ref _reservationDate, value);
    }

    public DateTime ReservationTime
    {
        get => _reservationTime;
        set => SetProperty(ref _reservationTime, value);
    }

    public string TableId
    {
        get => _tableId;
        set => SetProperty(ref _tableId, value);
    }

    public string NumberOfGuests
    {
        get => _numberOfGuests;
        set => SetProperty(ref _numberOfGuests, value);
    }

    public string SpecialRequests
    {
        get => _specialRequests;
        set => SetProperty(ref _specialRequests, value);
    }

    public string CancelReservationId
    {
        get => _cancelReservationId;
        set => SetProperty(ref _cancelReservationId, value);
    }

    public string ValidationErrors { get; private set; } = "";

    private bool IsValidReservationData()
    {
        return !string.IsNullOrWhiteSpace(CustomerName) &&
               !string.IsNullOrWhiteSpace(CustomerPhone) &&
               !string.IsNullOrWhiteSpace(CustomerEmail) &&
               !string.IsNullOrWhiteSpace(TableId) &&
               !string.IsNullOrWhiteSpace(NumberOfGuests) &&
               int.TryParse(NumberOfGuests, out _) &&
               int.TryParse(TableId, out _);
    }

    private ValidationResult ValidateReservation()
    {
        var reservation = new Reservation(
            0,
            CustomerName,
            CustomerPhone,
            CustomerEmail,
            ReservationDate,
            ReservationTime,
            int.Parse(TableId),
            int.Parse(NumberOfGuests),
            "pending",
            string.IsNullOrWhiteSpace(SpecialRequests) ? null : SpecialRequests
        );

        return _validator.Validate(reservation);
    }

    public IAsyncRelayCommand LoadReservationsCommand { get; }
    public IAsyncRelayCommand CreateReservationCommand { get; }
    public IAsyncRelayCommand CancelReservationCommand { get; }

    protected override void OnIsLoadingChanged()
    {
        LoadReservationsCommand.NotifyCanExecuteChanged();
        CreateReservationCommand.NotifyCanExecuteChanged();
        CancelReservationCommand.NotifyCanExecuteChanged();
    }

    protected override void OnPropertyChanged(string? propertyName)
    {
        base.OnPropertyChanged(propertyName);
        CreateReservationCommand.NotifyCanExecuteChanged();
        CancelReservationCommand.NotifyCanExecuteChanged();
    }

    private async Task LoadReservationsAsync()
    {
        IsLoading = true;
        StatusMessage = "";

        try
        {
            var reservations = await _reservationsUseCase.GetReservationsForDateAsync(SelectedDate, GetCancellationToken().Token);
            Reservations = reservations;
            StatusMessage = $"Loaded {reservations.Count} reservations for {SelectedDate:yyyy-MM-dd}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading reservations: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreateReservationAsync()
    {
        var validationResult = ValidateReservation();
        if (!validationResult.IsValid)
        {
            ValidationErrors = string.Join("\n", validationResult.Errors.Select(e => e.ErrorMessage));
            StatusMessage = $"Validation failed: {ValidationErrors}";
            return;
        }

        ValidationErrors = "";
        
        if (!int.TryParse(TableId, out var tableId) || !int.TryParse(NumberOfGuests, out var numberOfGuests))
        {
            StatusMessage = "Invalid table ID or number of guests.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = "";
            
            var reservation = await _reservationsUseCase.CreateReservationAsync(
                CustomerName,
                CustomerPhone,
                CustomerEmail,
                ReservationDate,
                ReservationTime,
                tableId,
                numberOfGuests,
                string.IsNullOrWhiteSpace(SpecialRequests) ? null : SpecialRequests,
                GetCancellationToken().Token
            );

            if (reservation is not null)
            {
                StatusMessage = $"Reservation {reservation.Id} created successfully.";
                ClearReservationForm();
                await LoadReservationsAsync();
            }
            else
            {
                StatusMessage = "Failed to create reservation.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating reservation: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CancelReservationAsync()
    {
        if (!int.TryParse(CancelReservationId, out var reservationId))
        {
            StatusMessage = "Invalid reservation ID.";
            return;
        }

        try
        {
            var result = await _reservationsUseCase.CancelReservationAsync(reservationId, GetCancellationToken().Token);

            if (result)
            {
                StatusMessage = $"Reservation {reservationId} cancelled successfully.";
                CancelReservationId = "";
                await LoadReservationsAsync();
            }
            else
            {
                StatusMessage = $"Failed to cancel reservation {reservationId}.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error cancelling reservation: {ex.Message}";
        }
    }

    private void ClearReservationForm()
    {
        CustomerName = "";
        CustomerPhone = "";
        CustomerEmail = "";
        ReservationDate = DateTime.Today;
        ReservationTime = DateTime.Now;
        TableId = "";
        NumberOfGuests = "2";
        SpecialRequests = "";
    }
}
