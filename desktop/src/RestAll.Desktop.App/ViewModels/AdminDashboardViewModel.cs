using System.Collections.ObjectModel;
using System.Windows.Input;
using RestAll.Desktop.Core.Admin;
using RestAll.Desktop.Core.Auth;

namespace RestAll.Desktop.App.ViewModels;

public class AdminDashboardViewModel : CancelableViewModelBase
{
    private readonly IManageStaffUseCase _staffUseCase;
    private readonly IGetAnalyticsSummaryUseCase _analyticsUseCase;
    private readonly IGetAuditLogsUseCase _logsUseCase;

    private ObservableCollection<UserProfile> _staffMembers = new();
    private UserProfile? _selectedStaffMember;
    private string _newStaffName = "";
    private string _newStaffEmail = "";
    private string _newStaffPassword = "";
    private string _newStaffRole = "waiter";
    private string _selectedStaffRole = "";
    private string _staffStatusMessage = "";

    private AnalyticsSummary? _analyticsSummary;
    private string _analyticsStatusMessage = "";

    private AuditLogPage? _auditLogPage;
    private string _auditLogActionFilter = "";
    private string _auditLogUserIdFilter = "";
    private DateTime? _auditLogDateFrom;
    private DateTime? _auditLogDateTo;
    private string _auditLogPerPage = "25";
    private string _logsStatusMessage = "";

    public AdminDashboardViewModel(
        IManageStaffUseCase staffUseCase,
        IGetAnalyticsSummaryUseCase analyticsUseCase,
        IGetAuditLogsUseCase logsUseCase)
    {
        _staffUseCase = staffUseCase;
        _analyticsUseCase = analyticsUseCase;
        _logsUseCase = logsUseCase;

        LoadStaffCommand = new RelayCommand(async () => await LoadStaffAsync());
        CreateStaffCommand = new RelayCommand(async () => await CreateStaffAsync(), () => CanCreateStaff());
        UpdateSelectedStaffRoleCommand = new RelayCommand(async () => await UpdateStaffRoleAsync(), () => _selectedStaffMember != null && !string.IsNullOrEmpty(_selectedStaffRole));
        DeleteSelectedStaffCommand = new RelayCommand(async () => await DeleteStaffAsync(), () => _selectedStaffMember != null);
        
        LoadAnalyticsCommand = new RelayCommand(async () => await LoadAnalyticsAsync());
        LoadLogsCommand = new RelayCommand(async () => await LoadLogsAsync());

        AvailableRoles = new ObservableCollection<string> { "admin", "manager", "waiter", "chef" };
    }

    // --- Staff properties ---
    public ObservableCollection<UserProfile> StaffMembers
    {
        get => _staffMembers;
        set => SetProperty(ref _staffMembers, value);
    }

    public UserProfile? SelectedStaffMember
    {
        get => _selectedStaffMember;
        set
        {
            if (SetProperty(ref _selectedStaffMember, value))
            {
                SelectedStaffRole = value?.Role ?? "";
                ((RelayCommand)UpdateSelectedStaffRoleCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteSelectedStaffCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string NewStaffName
    {
        get => _newStaffName;
        set { if (SetProperty(ref _newStaffName, value)) ((RelayCommand)CreateStaffCommand).RaiseCanExecuteChanged(); }
    }

    public string NewStaffEmail
    {
        get => _newStaffEmail;
        set { if (SetProperty(ref _newStaffEmail, value)) ((RelayCommand)CreateStaffCommand).RaiseCanExecuteChanged(); }
    }

    public string NewStaffPassword
    {
        get => _newStaffPassword;
        set { if (SetProperty(ref _newStaffPassword, value)) ((RelayCommand)CreateStaffCommand).RaiseCanExecuteChanged(); }
    }

    public string NewStaffRole
    {
        get => _newStaffRole;
        set => SetProperty(ref _newStaffRole, value);
    }

    public string SelectedStaffRole
    {
        get => _selectedStaffRole;
        set { if (SetProperty(ref _selectedStaffRole, value)) ((RelayCommand)UpdateSelectedStaffRoleCommand).RaiseCanExecuteChanged(); }
    }

    public string StaffStatusMessage
    {
        get => _staffStatusMessage;
        set => SetProperty(ref _staffStatusMessage, value);
    }

    public ObservableCollection<string> AvailableRoles { get; }

    // --- Analytics properties ---
    public AnalyticsSummary? AnalyticsSummary
    {
        get => _analyticsSummary;
        set => SetProperty(ref _analyticsSummary, value);
    }

    public string AnalyticsStatusMessage
    {
        get => _analyticsStatusMessage;
        set => SetProperty(ref _analyticsStatusMessage, value);
    }

    // --- Logs properties ---
    public AuditLogPage? AuditLogPage
    {
        get => _auditLogPage;
        set => SetProperty(ref _auditLogPage, value);
    }

    public string AuditLogActionFilter
    {
        get => _auditLogActionFilter;
        set => SetProperty(ref _auditLogActionFilter, value);
    }

    public string AuditLogUserIdFilter
    {
        get => _auditLogUserIdFilter;
        set => SetProperty(ref _auditLogUserIdFilter, value);
    }

    public DateTime? AuditLogDateFrom
    {
        get => _auditLogDateFrom;
        set => SetProperty(ref _auditLogDateFrom, value);
    }

    public DateTime? AuditLogDateTo
    {
        get => _auditLogDateTo;
        set => SetProperty(ref _auditLogDateTo, value);
    }

    public string AuditLogPerPage
    {
        get => _auditLogPerPage;
        set => SetProperty(ref _auditLogPerPage, value);
    }

    public string LogsStatusMessage
    {
        get => _logsStatusMessage;
        set => SetProperty(ref _logsStatusMessage, value);
    }

    // --- Commands ---
    public ICommand LoadStaffCommand { get; }
    public ICommand CreateStaffCommand { get; }
    public ICommand UpdateSelectedStaffRoleCommand { get; }
    public ICommand DeleteSelectedStaffCommand { get; }
    public ICommand LoadAnalyticsCommand { get; }
    public ICommand LoadLogsCommand { get; }

    // --- Actions ---
    private async Task LoadStaffAsync()
    {
        StaffStatusMessage = "Loading staff...";
        await ExecuteWithCancellationAsync(async (ct) =>
        {
            var staff = await _staffUseCase.GetStaffAsync(ct);
            StaffMembers = new ObservableCollection<UserProfile>(staff);
            StaffStatusMessage = $"Loaded {staff.Count} staff members.";
        }, "Error loading staff");
    }

    private bool CanCreateStaff() => 
        !string.IsNullOrWhiteSpace(NewStaffName) && 
        !string.IsNullOrWhiteSpace(NewStaffEmail) && 
        !string.IsNullOrWhiteSpace(NewStaffPassword);

    private async Task CreateStaffAsync()
    {
        StaffStatusMessage = "Creating staff member...";
        await ExecuteWithCancellationAsync(async (ct) =>
        {
            var newUser = await _staffUseCase.CreateStaffAsync(NewStaffName, NewStaffEmail, NewStaffPassword, NewStaffRole, ct);
            if (newUser != null)
            {
                StaffMembers.Add(newUser);
                NewStaffName = "";
                NewStaffEmail = "";
                NewStaffPassword = "";
                StaffStatusMessage = "Staff member created successfully.";
            }
            else
            {
                StaffStatusMessage = "Failed to create staff member.";
            }
        }, "Error creating staff member");
    }

    private async Task UpdateStaffRoleAsync()
    {
        if (SelectedStaffMember == null) return;
        
        StaffStatusMessage = $"Updating role for {SelectedStaffMember.Name}...";
        await ExecuteWithCancellationAsync(async (ct) =>
        {
            var updated = await _staffUseCase.UpdateStaffRoleAsync(SelectedStaffMember.Id, SelectedStaffRole, ct);
            if (updated != null)
            {
                var index = StaffMembers.IndexOf(SelectedStaffMember);
                if (index >= 0) StaffMembers[index] = updated;
                SelectedStaffMember = updated;
                StaffStatusMessage = "Role updated successfully.";
            }
            else
            {
                StaffStatusMessage = "Failed to update role.";
            }
        }, "Error updating role");
    }

    private async Task DeleteStaffAsync()
    {
        if (SelectedStaffMember == null) return;

        StaffStatusMessage = $"Deleting {SelectedStaffMember.Name}...";
        await ExecuteWithCancellationAsync(async (ct) =>
        {
            var success = await _staffUseCase.DeleteStaffAsync(SelectedStaffMember.Id, ct);
            if (success)
            {
                StaffMembers.Remove(SelectedStaffMember);
                SelectedStaffMember = null;
                StaffStatusMessage = "Staff member deleted.";
            }
            else
            {
                StaffStatusMessage = "Failed to delete staff member.";
            }
        }, "Error deleting staff member");
    }

    private async Task LoadAnalyticsAsync()
    {
        AnalyticsStatusMessage = "Loading analytics...";
        await ExecuteWithCancellationAsync(async (ct) =>
        {
            AnalyticsSummary = await _analyticsUseCase.GetSummaryAsync(ct);
            AnalyticsStatusMessage = "Analytics loaded.";
        }, "Error loading analytics");
    }

    private async Task LoadLogsAsync()
    {
        LogsStatusMessage = "Loading logs...";
        int.TryParse(AuditLogPerPage, out var perPage);
        if (perPage <= 0) perPage = 25;

        int? userId = null;
        if (int.TryParse(AuditLogUserIdFilter, out var uid)) userId = uid;

        var query = new AuditLogQuery(
            Action: string.IsNullOrWhiteSpace(AuditLogActionFilter) ? null : AuditLogActionFilter,
            UserId: userId,
            DateFrom: AuditLogDateFrom,
            DateTo: AuditLogDateTo,
            PerPage: perPage
        );

        await ExecuteWithCancellationAsync(async (ct) =>
        {
            AuditLogPage = await _logsUseCase.GetLogsAsync(query, ct);
            LogsStatusMessage = $"Loaded {AuditLogPage.Items.Count} logs.";
        }, "Error loading logs");
    }
}
