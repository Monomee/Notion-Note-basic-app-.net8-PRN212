# 📋 MÔ TẢ CHI TIẾT LUỒNG CHẠY DỰ ÁN NOTIONNOTE

## 🚀 1. KHỞI ĐỘNG ỨNG DỤNG (App.xaml.cs - OnStartup)

### Luồng khởi động:
1. **App.OnStartup()** được gọi khi ứng dụng khởi động
2. **Set ShutdownMode = OnExplicitShutdown** - Ứng dụng chỉ tắt khi gọi Shutdown() rõ ràng
3. **Seed Database** - Khởi tạo dữ liệu mẫu vào database
4. **Tạo và hiển thị LoginWindow**:
   ```csharp
   var loginWindow = new LoginWindow();
   var result = loginWindow.ShowDialog(); // Hiển thị dạng modal dialog
   ```
5. **Kiểm tra kết quả đăng nhập**:
   - Nếu `result == true` và có `AuthenticatedUser`:
     - Đổi ShutdownMode = OnMainWindowClose
     - Tạo MainWindow với userId
     - Hiển thị MainWindow
   - Nếu không: Shutdown ứng dụng

---

## 🔐 2. MÀN HÌNH ĐĂNG NHẬP (LoginWindow)

### 2.1. Khởi tạo LoginWindow (LoginWindow.xaml.cs)

**Luồng khởi tạo:**
```
LoginWindow() constructor
  ↓
InitializeComponent() - Load XAML
  ↓
Tạo NoteHubDbContext
  ↓
Tạo AuthService(context)
  ↓
Tạo LoginViewModel(authService)
  ↓
Set DataContext = _viewModel
  ↓
Subscribe PropertyChanged event để theo dõi AuthenticatedUser
```

### 2.2. Binding trong LoginWindow.xaml

#### **Username TextBox:**
```xml
<TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}"/>
```
- **Binding:** `Text` → `LoginViewModel.Username`
- **UpdateSourceTrigger:** PropertyChanged (cập nhật ngay khi gõ)
- **Khi người dùng gõ:**
  1. TextBox.Text thay đổi
  2. LoginViewModel.Username được set
  3. PropertyChanged event fire
  4. ErrorMessage được clear (trong setter của Username)

#### **Password PasswordBox:**
```xml
<PasswordBox PasswordChanged="PasswordBox_PasswordChanged"/>
```
- **Không dùng binding trực tiếp** (PasswordBox không hỗ trợ binding an toàn)
- **Event handler:** `PasswordBox_PasswordChanged`
- **Luồng:**
  1. Người dùng gõ password
  2. `PasswordChanged` event fire
  3. `LoginWindow.PasswordBox_PasswordChanged()` được gọi
  4. Set `_viewModel.Password = passwordBox.Password`
  5. LoginViewModel.Password setter fire PropertyChanged
  6. ErrorMessage được clear

#### **Error Message Border:**
```xml
<Border Visibility="{Binding ErrorMessage, Converter={StaticResource BooleanToVisibilityConverter}}">
  <TextBlock Text="{Binding ErrorMessage}"/>
</Border>
```
- **Binding:** `Visibility` → `ErrorMessage` qua converter
- **Converter:** `BooleanToVisibilityConverter` - Hiển thị nếu ErrorMessage không rỗng
- **Binding:** `Text` → `ErrorMessage`

#### **Mode Title:**
```xml
<TextBlock Text="{Binding ModeTitle}"/>
```
- **Binding:** `Text` → `ModeTitle` (computed property)
- **ModeTitle =** `IsLoginMode ? "Welcome Back" : "Create Account"`

#### **Login Button:**
```xml
<Button Content="Login" 
        Command="{Binding LoginCommand}"
        Visibility="{Binding IsLoginMode, Converter={StaticResource BooleanToVisibilityConverter}}"/>
```
- **Binding:** `Command` → `LoginCommand`
- **Binding:** `Visibility` → `IsLoginMode` (chỉ hiện khi IsLoginMode = true)

#### **Register Button:**
```xml
<Button Content="Create Account" 
        Command="{Binding RegisterCommand}"
        Visibility="{Binding IsLoginMode, Converter={StaticResource InverseBooleanToVisibilityConverter}}"/>
```
- **Binding:** `Command` → `RegisterCommand`
- **Binding:** `Visibility` → `IsLoginMode` (inverse - chỉ hiện khi IsLoginMode = false)

#### **Switch Mode Button:**
```xml
<Button Content="{Binding ModeSwitchText}"
        Command="{Binding SwitchModeCommand}"/>
```
- **Binding:** `Content` → `ModeSwitchText`
- **Binding:** `Command` → `SwitchModeCommand`

### 2.3. Luồng xử lý các nút bấm

#### **A. Nút Login (LoginCommand)**

**Luồng chạy:**
```
User click "Login" button
  ↓
LoginCommand.Execute() được gọi
  ↓
CanLogin() kiểm tra:
  - Username không rỗng?
  - Password không rỗng?
  - IsBusy = false?
  ↓ (nếu đúng)
Login() method được thực thi:
  1. Set IsBusy = true
  2. Clear ErrorMessage
  3. Gọi _authService.Login(Username, Password)
  4. Nếu thành công:
     - AuthenticatedUser = user
     - PropertyChanged("AuthenticatedUser") fire
  5. Nếu thất bại:
     - ErrorMessage = "Invalid username or password"
  6. Set IsBusy = false
  ↓
LoginWindow.PropertyChanged handler:
  - Nếu property = "AuthenticatedUser" và AuthenticatedUser != null
  - Set DialogResult = true
  - Window tự động đóng
  ↓
App.xaml.cs nhận DialogResult = true
  - Lấy AuthenticatedUser từ loginWindow
  - Tạo MainWindow(user.UserId)
```

#### **B. Nút Register (RegisterCommand)**

**Luồng chạy:**
```
User click "Create Account" button
  ↓
RegisterCommand.Execute() được gọi
  ↓
CanRegister() kiểm tra (tương tự CanLogin)
  ↓ (nếu đúng)
Register() method được thực thi:
  1. Set IsBusy = true
  2. Validate:
     - Username >= 3 ký tự
     - Password >= 3 ký tự
  3. Hiển thị MessageBox xác nhận
  4. Nếu user chọn Yes:
     - Gọi _authService.Register(Username, Password)
     - Nếu thành công:
       - Hiển thị MessageBox thông báo thành công
       - AuthenticatedUser = user
       - Window đóng và đăng nhập tự động
     - Nếu thất bại:
       - ErrorMessage = "Username already exists"
  5. Set IsBusy = false
```

#### **C. Nút Switch Mode (SwitchModeCommand)**

**Luồng chạy:**
```
User click "Don't have an account? Sign up" hoặc "Already have an account? Login"
  ↓
SwitchModeCommand.Execute() được gọi
  ↓
SwitchMode() method:
  1. IsLoginMode = !IsLoginMode
  2. PropertyChanged("IsLoginMode") fire
  3. PropertyChanged("ModeTitle") fire (computed property)
  4. PropertyChanged("ModeSwitchText") fire (computed property)
  5. Clear Username, Password, ErrorMessage
  ↓
UI tự động cập nhật:
  - Title thay đổi
  - Button Login/Register thay đổi visibility
  - Switch button text thay đổi
```

---

## 🏠 3. MÀN HÌNH CHÍNH (MainWindow)

### 3.1. Khởi tạo MainWindow (MainWindow.xaml.cs)

**Luồng khởi tạo:**
```
MainWindow(userId) constructor
  ↓
InitializeComponent() - Load XAML
  ↓
InitializeDataContext(userId):
  1. Tạo MainViewModel(userId)
  2. Set DataContext = viewModel
  3. Subscribe LogoutRequested event
```

### 3.2. Khởi tạo MainViewModel (MainViewModel.cs)

**Luồng khởi tạo:**
```
MainViewModel(userId) constructor
  ↓
Lưu _currentUserId = userId
  ↓
Tạo DbContext và Services:
  - _dbContext = new NoteHubDbContext()
  - _pageService = new PageService(_dbContext)
  - _workspaceService = new WorkspaceService(_dbContext)
  ↓
Tạo các Child ViewModels:
  - WorkSpaceListVM = new WorkSpaceListViewModel(_workspaceService)
  - PageListVM = new PageListViewModel(_pageService)
  - EditorVM = new EditorViewModel(_pageService)
  ↓
Subscribe Events:
  - WorkSpaceListVM.PropertyChanged → WorkSpaceListVM_PropertyChanged
  - PageListVM.PropertyChanged → PageListVM_PropertyChanged
  - EditorVM.PageUpdated → EditorVM_PageUpdated
  ↓
Tạo Commands:
  - LogoutCommand = new RelayCommand(Logout)
  ↓
LoadInitialData():
  1. Set WorkSpaceListVM.CurrentUserId = userId
  2. Execute RefreshCommand để load workspaces
  3. Nếu có workspace: Select workspace đầu tiên
```

### 3.3. Cấu trúc MainWindow.xaml

**Layout 3 cột:**
```xml
Grid.ColumnDefinitions:
  - Column 0: WorkSpaceListView (280px, min 360, max 400)
  - Column 1: Divider
  - Column 2: PageListView (320px, min 280, max 400)
  - Column 3: Divider
  - Column 4: EditorView (* - chiếm phần còn lại)
```

**Binding các View:**
```xml
<views:WorkSpaceListView DataContext="{Binding WorkSpaceListVM}"/>
<views:PageListView DataContext="{Binding PageListVM}"/>
<views:EditorView DataContext="{Binding EditorVM}"/>
```

**Logout Button:**
```xml
<Button Command="{Binding LogoutCommand}" Content="Log out"/>
```

### 3.4. Event Handlers trong MainViewModel

#### **A. WorkSpaceListVM_PropertyChanged**

**Kích hoạt khi:** `WorkSpaceListVM.Selected` thay đổi

**Luồng:**
```
User chọn workspace trong list
  ↓
WorkSpaceListVM.Selected = selectedWorkspace
  ↓
PropertyChanged("Selected") fire
  ↓
MainViewModel.WorkSpaceListVM_PropertyChanged() được gọi
  ↓
Xử lý:
  1. Clear EditorVM.CurrentPage = null (xóa page đang edit)
  2. Nếu Selected != null:
     - Set PageListVM.CurrentWorkspaceId = Selected.WorkspaceId
  3. Nếu Selected == null:
     - Set PageListVM.CurrentWorkspaceId = null
  ↓
PageListVM.CurrentWorkspaceId setter:
  - PropertyChanged("CurrentWorkspaceId") fire
  - Selected = null (clear selection)
  - RefreshPages() được gọi
  ↓
PageListVM.RefreshPages():
  - Load pages từ database theo WorkspaceId
  - Update FilteredPages
```

#### **B. PageListVM_PropertyChanged**

**Kích hoạt khi:** `PageListVM.Selected` thay đổi

**Luồng:**
```
User chọn page trong list
  ↓
PageListVM.Selected = selectedPage
  ↓
PropertyChanged("Selected") fire
  ↓
MainViewModel.PageListVM_PropertyChanged() được gọi
  ↓
Xử lý:
  1. Nếu Selected != null:
     - Load full page: _pageService.GetPageById(Selected.PageId)
     - Set EditorVM.CurrentPage = fullPage
  2. Nếu Selected == null:
     - Set EditorVM.CurrentPage = null
  ↓
EditorVM.CurrentPage setter:
  - PropertyChanged("CurrentPage") fire
  - LoadPageData() được gọi
  ↓
EditorVM.LoadPageData():
  - Title = CurrentPage.Title
  - Content = CurrentPage.Content
  - LastSavedAt = CurrentPage.UpdatedAt
  - IsDirty = false
  - UI tự động cập nhật qua binding
```

#### **C. EditorVM_PageUpdated**

**Kích hoạt khi:** Page được update (save, pin/unpin)

**Luồng:**
```
EditorVM update page (ví dụ: Save, Pin)
  ↓
PageUpdated?.Invoke(this, updatedPage) fire
  ↓
MainViewModel.EditorVM_PageUpdated() được gọi
  ↓
Xử lý:
  1. Tìm page trong PageListVM.Pages
  2. Update page data:
     - page.Title = updatedPage.Title
     - page.IsPinned = updatedPage.IsPinned
     - page.UpdatedAt = updatedPage.UpdatedAt
  3. Update PageItemViewModel.Title
  4. UpdateFilteredPages() - Re-sort (pinned lên đầu)
  5. Re-select page đã update
```

#### **D. Logout Command**

**Luồng:**
```
User click "Log out" button
  ↓
LogoutCommand.Execute() được gọi
  ↓
Logout() method:
  1. Hiển thị MessageBox xác nhận
  2. Nếu user chọn Yes:
     - Fire LogoutRequested event
  ↓
MainWindow.ViewModel_LogoutRequested() được gọi:
  1. Set ShutdownMode = OnExplicitShutdown
  2. Close MainWindow
  3. Tạo LoginWindow mới
  4. ShowDialog()
  5. Nếu đăng nhập thành công:
     - Tạo MainWindow mới với user mới
     - Show MainWindow
  6. Nếu không: Shutdown app
```

---

## 📁 4. WORKSPACE LIST VIEW (WorkSpaceListView)

### 4.1. Binding trong WorkSpaceListView.xaml

#### **Search TextBox:**
```xml
<TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"/>
```
- **Binding:** `Text` → `WorkSpaceListViewModel.SearchText`
- **Khi gõ:** SearchText setter → UpdateFilteredWorkspaces() → Filter list

#### **Workspace ListBox:**
```xml
<ListBox ItemsSource="{Binding FilteredWorkspaces}"
         SelectedItem="{Binding Selected, Mode=TwoWay}"/>
```
- **Binding:** `ItemsSource` → `FilteredWorkspaces` (ObservableCollection)
- **Binding:** `SelectedItem` → `Selected` (TwoWay - UI và VM đồng bộ)

#### **Workspace Item Display:**
```xml
<TextBlock Text="{Binding Name}"/>
```
- **Binding:** `Text` → `WorkspaceItemViewModel.Name`
- **Edit Mode:** TextBox với `IsEditing` trigger

### 4.2. Các nút bấm trong WorkSpaceListView

#### **A. + Add Workspace Button**

**Binding:**
```xml
<Button Command="{Binding AddWorkspaceCommand}" Content="+ Add Workspace"/>
```

**Luồng:**
```
User click "+ Add Workspace"
  ↓
AddWorkspaceCommand.Execute() được gọi
  ↓
CanAddWorkspace() kiểm tra: !IsBusy
  ↓ (nếu đúng)
AddWorkspace() method:
  1. Set IsBusy = true
  2. Tạo Workspace mới:
     - Name = "New Workspace"
     - CreatedAt = DateTime.Now
     - UserId = CurrentUserId
  3. Gọi _workspaceService.CreateWorkspace()
  4. Tạo WorkspaceItemViewModel
  5. Insert vào đầu _workspaces collection
  6. UpdateFilteredWorkspaces()
  7. Selected = workspaceItem mới
  8. Set IsEditing = true (cho phép đổi tên ngay)
  9. Set IsBusy = false
  ↓
UI tự động cập nhật:
  - Workspace mới xuất hiện trong list
  - TextBox edit mode được kích hoạt
  - MainViewModel nhận Selected thay đổi → Load pages
```

#### **B. ✏️ Rename Button**

**Binding:**
```xml
<Button Command="{Binding RenameWorkspaceCommand}" Content="✏️ Rename"/>
```

**Luồng:**
```
User click "✏️ Rename"
  ↓
RenameWorkspaceCommand.Execute() được gọi
  ↓
CanRenameWorkspace() kiểm tra: Selected != null && !IsBusy
  ↓ (nếu đúng)
RenameWorkspace() method:
  - Set Selected.IsEditing = true
  ↓
UI tự động chuyển sang edit mode:
  - TextBlock ẩn
  - TextBox hiện
  - User có thể sửa tên
  ↓
Khi user nhấn Enter/Escape hoặc LostFocus:
  - TextBox_LostFocus hoặc TextBox_KeyDown handler
  - Set IsEditing = false
  ↓
WorkspaceItemViewModel.IsEditing setter:
  - Nếu tên thay đổi:
    - _workspace.Name = _name
    - _workspaceService.UpdateWorkspace(_workspace)
  - PropertyChanged fire
```

#### **C. 🗑️ Delete Button**

**Binding:**
```xml
<Button Command="{Binding DeleteWorkspaceCommand}" Content="🗑️ Delete"/>
```

**Luồng:**
```
User click "🗑️ Delete"
  ↓
DeleteWorkspaceCommand.Execute() được gọi
  ↓
CanDeleteWorkspace() kiểm tra: Selected != null && !IsBusy
  ↓ (nếu đúng)
DeleteWorkspace() method:
  1. Hiển thị MessageBox xác nhận
  2. Nếu user chọn Yes:
     - Set IsBusy = true
     - Gọi _workspaceService.DeleteWorkspace(Selected.WorkspaceId)
     - Remove khỏi _workspaces collection
     - UpdateFilteredWorkspaces()
     - Selected = null
     - Set IsBusy = false
  ↓
UI tự động cập nhật:
  - Workspace biến mất khỏi list
  - PageListVM.CurrentWorkspaceId = null → Clear pages
  - EditorVM.CurrentPage = null → Clear editor
```

#### **D. Double-click Workspace**

**Event Handler:**
```csharp
ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
```

**Luồng:**
```
User double-click workspace
  ↓
ListBox_MouseDoubleClick() được gọi
  ↓
Hiện tại: Không có logic đặc biệt
  - Selection đã được xử lý qua TwoWay binding
  - Có thể thêm logic edit mode nếu cần
```

---

## 📄 5. PAGE LIST VIEW (PageListView)

### 5.1. Binding trong PageListView.xaml

#### **Search TextBox:**
```xml
<TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"/>
```
- **Binding:** `Text` → `PageListViewModel.SearchText`
- **Khi gõ:** SearchText setter → UpdateFilteredPages() → Filter và sort

#### **Page ListBox:**
```xml
<ListBox ItemsSource="{Binding FilteredPages}"
         SelectedItem="{Binding Selected, Mode=TwoWay}"
         SelectionChanged="ListBox_SelectionChanged"/>
```
- **Binding:** `ItemsSource` → `FilteredPages`
- **Binding:** `SelectedItem` → `Selected` (TwoWay)
- **Event:** `SelectionChanged` (hiện tại chỉ để logging)

#### **Page Item Display:**
```xml
<TextBlock Text="{Binding Title}"/>
```
- **Binding:** `Text` → `PageItemViewModel.Title`
- **Icon:** 📄 (unpinned) hoặc 📌 (pinned) - Binding `IsPinned`

### 5.2. Các nút bấm trong PageListView

#### **A. + Add Page Button**

**Binding:**
```xml
<Button Command="{Binding AddPageCommand}" Content="+ Add Page"/>
```

**Luồng:**
```
User click "+ Add Page"
  ↓
AddPageCommand.Execute() được gọi
  ↓
CanAddPage() kiểm tra: CurrentWorkspaceId != null && !IsBusy
  ↓ (nếu đúng)
AddPage() method:
  1. Set IsBusy = true
  2. Tạo Page mới:
     - Title = "Untitled Page"
     - Content = ""
     - CreatedAt = DateTime.Now
     - UpdatedAt = DateTime.Now
     - IsPinned = false
     - WorkspaceId = CurrentWorkspaceId.Value
  3. Gọi _pageService.CreatePage()
  4. Tạo PageItemViewModel
  5. Add vào _pages collection
  6. UpdateFilteredPages() - Sort (pinned lên đầu)
  7. Selected = pageItem mới
  8. Set IsEditing = true (cho phép đổi tên ngay)
  9. Set IsBusy = false
  ↓
UI tự động cập nhật:
  - Page mới xuất hiện trong list
  - TextBox edit mode được kích hoạt
  - MainViewModel nhận Selected thay đổi
  ↓
MainViewModel.PageListVM_PropertyChanged():
  - Load full page từ database
  - Set EditorVM.CurrentPage = fullPage
  ↓
EditorVM.LoadPageData():
  - Title và Content được load vào editor
  - User có thể edit ngay
```

#### **B. 🗑️ Delete Button**

**Binding:**
```xml
<Button Command="{Binding DeletePageCommand}" Content="🗑️ Delete"/>
```

**Luồng:**
```
User click "🗑️ Delete"
  ↓
DeletePageCommand.Execute() được gọi
  ↓
CanDeletePage() kiểm tra: Selected != null && !IsBusy
  ↓ (nếu đúng)
DeletePage() method:
  1. Hiển thị MessageBox xác nhận
  2. Nếu user chọn Yes:
     - Set IsBusy = true
     - Gọi _pageService.DeletePage(Selected.PageId)
     - Remove khỏi _pages collection
     - UpdateFilteredPages()
     - Selected = null
     - Set IsBusy = false
  ↓
UI tự động cập nhật:
  - Page biến mất khỏi list
  - MainViewModel nhận Selected = null
  - EditorVM.CurrentPage = null → Clear editor
```

#### **C. Click/Select Page**

**Luồng:**
```
User click page trong list
  ↓
ListBox.SelectedItem thay đổi
  ↓
TwoWay binding: PageListVM.Selected = selectedPage
  ↓
PropertyChanged("Selected") fire
  ↓
MainViewModel.PageListVM_PropertyChanged() được gọi
  ↓
Load full page và set EditorVM.CurrentPage
  ↓
Editor hiển thị page content
```

---

## ✏️ 6. EDITOR VIEW (EditorView)

### 6.1. Binding trong EditorView.xaml

#### **Title TextBox:**
```xml
<TextBox Text="{Binding Title, UpdateSourceTrigger=PropertyChanged}"/>
```
- **Binding:** `Text` → `EditorViewModel.Title`
- **UpdateSourceTrigger:** PropertyChanged (cập nhật ngay khi gõ)
- **Khi gõ:** Title setter → SetDirty() → IsDirty = true

#### **Content TextBox:**
```xml
<TextBox Text="{Binding Content, UpdateSourceTrigger=PropertyChanged}"
         AcceptsReturn="True" AcceptsTab="True"/>
```
- **Binding:** `Text` → `EditorViewModel.Content`
- **UpdateSourceTrigger:** PropertyChanged
- **Khi gõ:** Content setter → SetDirty() → IsDirty = true

#### **Pin Button:**
```xml
<Button Command="{Binding PinCommand}" Content="📌 Pin/Unpin"/>
```
- **Binding:** `Command` → `PinCommand`
- **IsEnabled:** `!IsBusy` (qua InverseBooleanConverter)

#### **Save Button:**
```xml
<Button Command="{Binding SaveCommand}" Content="💾 Save"/>
```
- **Binding:** `Command` → `SaveCommand`
- **IsEnabled:** `!IsBusy`

#### **Status Bar:**
```xml
<TextBlock Text="{Binding IsDirty, StringFormat=Dirty: {0}}"/>
<TextBlock Text="{Binding LastSavedAt, StringFormat=Last saved: {0:HH:mm:ss}}"/>
```
- **Binding:** `Text` → `IsDirty` và `LastSavedAt`

### 6.2. Các nút bấm trong EditorView

#### **A. 📌 Pin/Unpin Button**

**Luồng:**
```
User click "📌 Pin/Unpin"
  ↓
PinCommand.Execute() được gọi
  ↓
CanPinPage() kiểm tra: CurrentPage != null
  ↓ (nếu đúng)
PinPage() method:
  1. Set IsBusy = true
  2. Toggle: CurrentPage.IsPinned = !CurrentPage.IsPinned
  3. CurrentPage.UpdatedAt = DateTime.Now
  4. Gọi _pageService.UpdatePage(CurrentPage)
  5. PropertyChanged("CurrentPage") fire
  6. Fire PageUpdated event
  7. Set IsBusy = false
  ↓
MainViewModel.EditorVM_PageUpdated() được gọi:
  - Update page trong PageListVM
  - UpdateFilteredPages() - Re-sort (pinned lên đầu)
  - Re-select page
  ↓
UI tự động cập nhật:
  - Icon trong PageList thay đổi (📄 ↔ 📌)
  - Page di chuyển lên đầu list nếu pinned
```

#### **B. 💾 Save Button**

**Luồng:**
```
User click "💾 Save"
  ↓
SaveCommand.Execute() được gọi
  ↓
CanSavePage() kiểm tra: Title hoặc Content không rỗng
  ↓ (nếu đúng)
SavePage() method:
  1. Set IsBusy = true
  2. Nếu CurrentPage != null (đang edit page có sẵn):
     - CurrentPage.Title = Title
     - CurrentPage.Content = Content
     - CurrentPage.UpdatedAt = DateTime.Now
     - Gọi _pageService.UpdatePage(CurrentPage)
     - LastSavedAt = DateTime.Now
     - IsDirty = false
     - Fire PageUpdated event
  3. Nếu CurrentPage == null (tạo page mới):
     - Tạo Page mới với Title, Content
     - Gọi _pageService.CreatePage()
     - Set CurrentPage = createdPage
     - LastSavedAt = DateTime.Now
     - IsDirty = false
     - Fire PageUpdated event
  4. Set IsBusy = false
  ↓
MainViewModel.EditorVM_PageUpdated() được gọi:
  - Update page trong PageListVM
  - UpdateFilteredPages()
  - Re-select page
  ↓
UI tự động cập nhật:
  - Status bar: "Dirty: False"
  - LastSavedAt hiển thị thời gian
  - Page title trong list được cập nhật
```

#### **C. Auto-save khi edit**

**Luồng:**
```
User gõ vào Title hoặc Content TextBox
  ↓
TextBox.Text thay đổi
  ↓
Binding: EditorViewModel.Title/Content được set
  ↓
Title/Content setter:
  1. Set value
  2. PropertyChanged fire
  3. SetDirty() được gọi
  4. IsDirty = true
  ↓
UI tự động cập nhật:
  - Status bar: "Dirty: True" (màu vàng)
  - Save button vẫn enabled
  ↓
User phải click Save để lưu
  (Không có auto-save tự động)
```

---

## 🔄 7. TỔNG HỢP LUỒNG TƯƠNG TÁC

### 7.1. Luồng chọn Workspace → Load Pages → Edit Page

```
1. User chọn Workspace trong WorkSpaceListView
   ↓
2. WorkSpaceListVM.Selected = workspace
   ↓
3. MainViewModel.WorkSpaceListVM_PropertyChanged()
   - Set PageListVM.CurrentWorkspaceId = workspace.WorkspaceId
   - Clear EditorVM.CurrentPage
   ↓
4. PageListVM.CurrentWorkspaceId setter
   - RefreshPages() được gọi
   - Load pages từ database
   - Update FilteredPages
   ↓
5. User chọn Page trong PageListView
   ↓
6. PageListVM.Selected = page
   ↓
7. MainViewModel.PageListVM_PropertyChanged()
   - Load full page: _pageService.GetPageById(page.PageId)
   - Set EditorVM.CurrentPage = fullPage
   ↓
8. EditorVM.CurrentPage setter
   - LoadPageData() được gọi
   - Title = page.Title
   - Content = page.Content
   - UI hiển thị page content
```

### 7.2. Luồng tạo Page mới

```
1. User click "+ Add Page"
   ↓
2. PageListVM.AddPage()
   - Tạo Page mới trong database
   - Add vào Pages collection
   - Selected = newPage
   - IsEditing = true (để đổi tên)
   ↓
3. MainViewModel.PageListVM_PropertyChanged()
   - Load full page
   - Set EditorVM.CurrentPage = newPage
   ↓
4. EditorVM.LoadPageData()
   - Title = "Untitled Page"
   - Content = ""
   - User có thể edit ngay
```

### 7.3. Luồng Save Page

```
1. User edit Title/Content trong EditorView
   ↓
2. Title/Content setter → IsDirty = true
   ↓
3. User click "💾 Save"
   ↓
4. EditorVM.SavePage()
   - Update page trong database
   - IsDirty = false
   - Fire PageUpdated event
   ↓
5. MainViewModel.EditorVM_PageUpdated()
   - Update page trong PageListVM
   - UpdateFilteredPages() - Re-sort
   - Re-select page
```

### 7.4. Luồng Pin/Unpin Page

```
1. User click "📌 Pin/Unpin"
   ↓
2. EditorVM.PinPage()
   - Toggle IsPinned
   - Update database
   - Fire PageUpdated event
   ↓
3. MainViewModel.EditorVM_PageUpdated()
   - Update page trong PageListVM
   - UpdateFilteredPages() - Re-sort (pinned lên đầu)
   - Re-select page
   ↓
4. UI tự động cập nhật:
   - Icon thay đổi (📄 ↔ 📌)
   - Page di chuyển lên đầu list
```

---

## 📊 8. DATA BINDING TỔNG HỢP

### 8.1. LoginWindow Bindings

| UI Element | Binding Path | Binding Mode | Update Trigger |
|------------|--------------|--------------|----------------|
| Username TextBox | `Username` | TwoWay | PropertyChanged |
| Password PasswordBox | (Event handler) | - | - |
| Error Message | `ErrorMessage` | OneWay | - |
| Mode Title | `ModeTitle` | OneWay | - |
| Login Button Command | `LoginCommand` | OneWay | - |
| Login Button Visibility | `IsLoginMode` | OneWay | - |
| Register Button Command | `RegisterCommand` | OneWay | - |
| Register Button Visibility | `IsLoginMode` (inverse) | OneWay | - |
| Switch Mode Button | `ModeSwitchText` | OneWay | - |
| Switch Mode Command | `SwitchModeCommand` | OneWay | - |

### 8.2. MainWindow Bindings

| UI Element | Binding Path | Binding Mode | Update Trigger |
|------------|--------------|--------------|----------------|
| WorkSpaceListView DataContext | `WorkSpaceListVM` | OneWay | - |
| PageListView DataContext | `PageListVM` | OneWay | - |
| EditorView DataContext | `EditorVM` | OneWay | - |
| Logout Button Command | `LogoutCommand` | OneWay | - |

### 8.3. WorkSpaceListView Bindings

| UI Element | Binding Path | Binding Mode | Update Trigger |
|------------|--------------|--------------|----------------|
| Search TextBox | `SearchText` | TwoWay | PropertyChanged |
| ListBox ItemsSource | `FilteredWorkspaces` | OneWay | - |
| ListBox SelectedItem | `Selected` | TwoWay | - |
| Workspace Name | `Name` | OneWay | - |
| Add Button Command | `AddWorkspaceCommand` | OneWay | - |
| Rename Button Command | `RenameWorkspaceCommand` | OneWay | - |
| Delete Button Command | `DeleteWorkspaceCommand` | OneWay | - |

### 8.4. PageListView Bindings

| UI Element | Binding Path | Binding Mode | Update Trigger |
|------------|--------------|--------------|----------------|
| Search TextBox | `SearchText` | TwoWay | PropertyChanged |
| ListBox ItemsSource | `FilteredPages` | OneWay | - |
| ListBox SelectedItem | `Selected` | TwoWay | - |
| Page Title | `Title` | OneWay | - |
| Page Icon | `IsPinned` | OneWay | - |
| Add Button Command | `AddPageCommand` | OneWay | - |
| Delete Button Command | `DeletePageCommand` | OneWay | - |

### 8.5. EditorView Bindings

| UI Element | Binding Path | Binding Mode | Update Trigger |
|------------|--------------|--------------|----------------|
| Title TextBox | `Title` | TwoWay | PropertyChanged |
| Content TextBox | `Content` | TwoWay | PropertyChanged |
| Pin Button Command | `PinCommand` | OneWay | - |
| Save Button Command | `SaveCommand` | OneWay | - |
| IsDirty Status | `IsDirty` | OneWay | - |
| LastSavedAt | `LastSavedAt` | OneWay | - |
| IsEmpty Visibility | `IsEmpty` | OneWay | - |
| CurrentPage Visibility | `CurrentPage` | OneWay | - |

---

## 🎯 9. CÁC CONVERTER ĐƯỢC SỬ DỤNG

### 9.1. BooleanToVisibilityConverter
- **Mục đích:** Convert bool → Visibility
- **Sử dụng trong:**
  - ErrorMessage visibility (LoginWindow)
  - Login/Register button visibility
  - Empty state visibility
  - IsEmpty visibility trong EditorView

### 9.2. InverseBooleanConverter
- **Mục đích:** Convert bool → !bool
- **Sử dụng trong:**
  - Button IsEnabled (khi IsBusy = true thì disabled)

### 9.3. InverseBooleanToVisibilityConverter
- **Mục đích:** Convert bool → Visibility (inverse)
- **Sử dụng trong:**
  - Register button visibility (khi IsLoginMode = false)

---

## 🔗 10. EVENT FLOW DIAGRAM

```
┌─────────────────┐
│   App.xaml.cs   │
│   OnStartup()   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  LoginWindow    │
│  ShowDialog()   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐      ┌──────────────────┐
│ LoginViewModel  │◄─────│   AuthService     │
│  Login/Register │      │  Login/Register   │
└────────┬────────┘      └──────────────────┘
         │
         │ AuthenticatedUser set
         ▼
┌─────────────────┐
│  MainWindow     │
│  (userId)       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ MainViewModel   │
│  (userId)       │
└────────┬────────┘
         │
         ├──────────────┬──────────────┐
         ▼              ▼              ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│WorkSpaceList│ │ PageList     │ │ Editor        │
│ViewModel    │ │ViewModel     │ │ViewModel      │
└─────────────┘ └──────────────┘ └──────────────┘
         │              │              │
         │              │              │
         └──────────────┴──────────────┘
                    │
                    ▼
         ┌──────────────────────┐
         │  Event Handlers      │
         │  - PropertyChanged   │
         │  - PageUpdated       │
         └──────────────────────┘
```

---

## 📝 11. TÓM TẮT CÁC LUỒNG CHÍNH

### 11.1. Luồng đăng nhập
1. App khởi động → Show LoginWindow
2. User nhập username/password
3. Click Login → AuthService.Login()
4. Nếu thành công → AuthenticatedUser set → Window đóng
5. App tạo MainWindow với userId

### 11.2. Luồng làm việc với Workspace
1. MainWindow load → LoadInitialData() → Load workspaces
2. User chọn workspace → Selected thay đổi
3. MainViewModel nhận event → Set PageListVM.CurrentWorkspaceId
4. PageListVM load pages của workspace đó

### 11.3. Luồng làm việc với Page
1. User chọn page → Selected thay đổi
2. MainViewModel nhận event → Load full page → Set EditorVM.CurrentPage
3. EditorVM load page data → Hiển thị trong editor
4. User edit → IsDirty = true
5. User save → Update database → Fire PageUpdated event
6. MainViewModel nhận event → Update PageListVM → Re-sort

### 11.4. Luồng tạo mới
- **Workspace:** Add → Create → Insert vào list → Select → Edit mode
- **Page:** Add → Create → Insert vào list → Select → Load vào editor → Edit mode

### 11.5. Luồng xóa
- **Workspace:** Delete → Confirm → Delete từ DB → Remove khỏi list → Clear pages
- **Page:** Delete → Confirm → Delete từ DB → Remove khỏi list → Clear editor

---

**Kết thúc tài liệu mô tả luồng chạy dự án NotionNote**

