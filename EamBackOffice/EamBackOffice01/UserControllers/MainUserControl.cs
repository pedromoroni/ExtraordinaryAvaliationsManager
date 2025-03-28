using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BCrypt.Net;
using EamBackOffice01.Models;
using Microsoft.EntityFrameworkCore;
using static System.ComponentModel.Design.ObjectSelectorEditor;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EamBackOffice01.UserControllers;

/* TODO:
 * ver requests de cada um e ainda o historico
 * fazer tudo tal como o user creation, com caixas de mensagens
 * fazer a cena do refresh e blas blas para todos os listviews
 */

public partial class MainUserControl : UserControl {
    private enum AppTabPage {
        Spectator,
        UserCreation,
        SituationCreation,
        ClassCreation,
        CourseCreation,
        SubjectCreation,
    }

    private int StudentRoleId {
        get {
            using var dbContext = new EamDbContext();

            foreach (Role role in dbContext.Roles) {
                if (role.Name == "Student") {
                    return role.Id;
                }
            }
            return 0;
        }
    }
    private int TeacherRoleId {
        get {
            using var dbContext = new EamDbContext();

            foreach (Role role in dbContext.Roles) {
                if (role.Name == "Teacher") {
                    return role.Id;
                }
            }
            return 0;
        }
    }
    private int SecretaryRoleId {
        get {
            using var dbContext = new EamDbContext();

            foreach (Role role in dbContext.Roles) {
                if (role.Name == "Secretary") {
                    return role.Id;
                }
            }
            return 0;
        }
    }

    private int PendingApprovalStatusId {
        get {
            using var dbContext = new EamDbContext();

            foreach (Status status in dbContext.Statuses) {
                if (status.Description == "Pending Approval") {
                    return status.Id;
                }
            }
            return 0;
        }
    }
    private int NotApprovedStatusId {
        get {
            using var dbContext = new EamDbContext();

            foreach (Status status in dbContext.Statuses) {
                if (status.Description == "Not Approved") {
                    return status.Id;
                }
            }
            return 0;
        }
    }
    private int PendingPaymentStatusId {
        get {
            using var dbContext = new EamDbContext();

            foreach (Status status in dbContext.Statuses) {
                if (status.Description == "Pending Payment") {
                    return status.Id;
                }
            }
            return 0;
        }
    }
    private int PaidStatusId {
        get {
            using var dbContext = new EamDbContext();

            foreach (Status status in dbContext.Statuses) {
                if (status.Description == "Paid") {
                    return status.Id;
                }
            }
            return 0;
        }
    }
    private int ReleasedStatusId {
        get {
            using var dbContext = new EamDbContext();

            foreach (Status status in dbContext.Statuses) {
                if (status.Description == "Released") {
                    return status.Id;
                }
            }
            return 0;
        }
    }
    private int CanceledStatusId {
        get {
            using var dbContext = new EamDbContext();

            foreach (Status status in dbContext.Statuses) {
                if (status.Description == "Canceled") {
                    return status.Id;
                }
            }
            return 0;
        }
    }

    private (int x, int y)? _bufferPoint = null;
    private Module? _bufferEditModule = null;
    private User? _bufferEditUser = null;
    private Situation? _bufferEditSituation = null;
    private Class? _bufferEditClass = null;
    private Course? _bufferEditCourse = null;
    private Subject? _bufferEditSubject = null;
    private List<Module> _bufferAddedModules = [];
    private List<Subject> _bufferAddedSubjects = [];
    private List<User> _bufferAddedTeachers = [];
    private List<User> _bufferAddedStudents = [];
    private AppTabPage _currentTabPage;

    private User? _bufferUserRequests = null;
    private User? _bufferStudentRequests = null;
    private User? _bufferTeacherRequests = null;
    private User? _bufferSecretaryRequests = null;
    private Subject? _bufferSubjectRequests = null;
    private Module? _bufferModuleRequests = null;
    private Course? _bufferCourseRequests = null;

    public MainUserControl() {
        InitializeComponent();
    }

    private void MainUserControl_Load(object sender, EventArgs e) {
        mainTabControl_SelectedIndexChanged(sender, e);
        spectatorComboBox.SelectedIndex = 0;
    }

    private void mainTabControl_SelectedIndexChanged(object sender, EventArgs e) {
        errorProvider.Clear();
        _bufferPoint = null;
        _bufferEditModule = null;
        _bufferEditUser = null;
        _bufferAddedModules.Clear();
        _bufferAddedSubjects.Clear();
        _bufferAddedTeachers.Clear();
        _bufferAddedStudents.Clear();

        foreach (TabPage tb in mainTabControl.TabPages) {
            ClearTabPage(tb);
        }

        using var dbContext = new EamDbContext();

        switch (mainTabControl.SelectedIndex) {

        case (int)AppTabPage.Spectator:
            _currentTabPage = AppTabPage.Spectator;

            string? selected = spectatorComboBox.SelectedItem?.ToString();

            string[] options = [
                "User",
                .. dbContext.Roles.Select(r => r.Name),
                "Class",
                "Course",
                "Subject",
                "Module",
                "Situation",
                "Request",
            ];
            Helper.InjectToComboBox(spectatorComboBox, options);
            spectatorComboBox.SelectedItem = selected;
            break;

        case (int)AppTabPage.UserCreation:
            _currentTabPage = AppTabPage.UserCreation;

            Helper.InjectToComboBox(classComboBox, dbContext.Classes.Where(c => !c.IsDeleted), c => c.Name);
            break;

        case (int)AppTabPage.ClassCreation:
            _currentTabPage = AppTabPage.ClassCreation;

            IEnumerable<User> students = dbContext.Users
                .Where(u => !u.IsDeleted)
                .Where(u => u.RoleId == StudentRoleId)
                .Where(u => classShowAllStudentsCheckBox.Checked ? true : u.Class == null)
                .Include(u => u.Class);

            IEnumerable<(ColumnHeader, Expression<Func<User, object>>)> student_selectors = [
                ( new ColumnHeader { Text = "ID", Width = 70, TextAlign = HorizontalAlignment.Right }
                , s => s.Identification ),
                ( new ColumnHeader { Text = "Name", Width = 200, TextAlign = HorizontalAlignment.Left }
                , s => s.FirstName + " " + s.LastName),
                ( new ColumnHeader { Text = "Email", Width = 90, TextAlign = HorizontalAlignment.Left }
                , s => s.Email ),
                ( new ColumnHeader { Text = "Class", Width = 90, TextAlign = HorizontalAlignment.Left }
                , s => s.Class == null ? "Undefined" : s.Class.Name ),
            ];
            classStudentsListView.ItemChecked -= classStudentsListView_ItemChecked;
            Helper.InjectToListView(classStudentsListView, students, student_selectors);
            classStudentsListView.ItemChecked += classStudentsListView_ItemChecked;

            for (int i = 0; i < students.Count(); i++) {
                classStudentsListView.Items[i].Tag = students.ElementAt(i).Id;
            }
            break;

        case (int)AppTabPage.CourseCreation:
            _currentTabPage = AppTabPage.CourseCreation;

            IEnumerable<Subject> subjects = dbContext.Subjects
                .Where(s => !s.IsDeleted)
                .Include(s => s.Modules);

            IEnumerable<(ColumnHeader, Expression<Func<Subject, object>>)> subject_selectors = [
                ( new ColumnHeader { Text = "Abbreviation", Width = 90, TextAlign = HorizontalAlignment.Left }
                , s => s.Abbreviation ),
                ( new ColumnHeader { Text = "Name", Width = 550, TextAlign = HorizontalAlignment.Left }
                , s => s.Name ),
                ( new ColumnHeader { Text = "Modules", Width = 70, TextAlign = HorizontalAlignment.Right }
                , s => s.Modules.Count ),
                ( new ColumnHeader { Text = "Duration", Width = 90, TextAlign = HorizontalAlignment.Right }
                , s => IntToTimeString(s.Modules.Sum(m => m.DurationMin)) ),
            ];

            courseSubjectsListView.ItemChecked -= courseSubjectsListView_ItemChecked;
            Helper.InjectToListView(courseSubjectsListView, subjects, subject_selectors);
            courseSubjectsListView.ItemChecked += courseSubjectsListView_ItemChecked;

            for (int i = 0; i < subjects.Count(); i++) {
                courseSubjectsListView.Items[i].Tag = subjects.ElementAt(i).Id;
            }
            break;

        case (int)AppTabPage.SubjectCreation:
            _currentTabPage = AppTabPage.SubjectCreation;

            IEnumerable<User> teachers = dbContext.Users
                .Where(u => !u.IsDeleted)
                .Where(t => t.RoleId == TeacherRoleId)
                .Include(t => t.Subjects);

            IEnumerable<(ColumnHeader, Expression<Func<User, object>>)> teacher_selectors = [
                ( new ColumnHeader { Text = "ID", Width = 80, TextAlign = HorizontalAlignment.Right }
                , t => t.Identification ),
                ( new ColumnHeader { Text = "Name", Width = 200, TextAlign = HorizontalAlignment.Left }
                , t => t.FirstName + " " + t.LastName ),
                ( new ColumnHeader { Text = "Subjects", Width = 70, TextAlign = HorizontalAlignment.Right }
                , t => t.Subjects.Count ),
            ];

            subjectTeachersListView.ItemChecked -= subjectTeachersListView_ItemChecked;
            Helper.InjectToListView(subjectTeachersListView, teachers, teacher_selectors);
            subjectTeachersListView.ItemChecked += subjectTeachersListView_ItemChecked;

            for (int i = 0; i < teachers.Count(); i++) {
                subjectTeachersListView.Items[i].Tag = teachers.ElementAt(i).Id;
            }
            break;
        }
    }

    private void classComboBox_DropDown(object sender, EventArgs e) {
        string? selected = classComboBox.SelectedItem?.ToString();

        using var dbContext = new EamDbContext();
        Helper.InjectToComboBox(classComboBox, dbContext.Classes.Where(c => !c.IsDeleted), c => c.Name);

        classComboBox.SelectedItem = selected;
    }

    private void roleComboBox_DropDown(object sender, EventArgs e) {
        string? selected = roleComboBox.SelectedItem?.ToString();

        using var dbContext = new EamDbContext();
        Helper.InjectToComboBox(roleComboBox, dbContext.Roles, r => r.Name);

        roleComboBox.SelectedItem = selected;
    }

    private void userConfirmButton_Click(object sender, EventArgs e) {
        errorProvider.Clear();

        identificationTextBox.Text = identificationTextBox.Text.Trim();
        emailTextBox.Text = emailTextBox.Text.Trim();
        emailTextBox.Text = emailTextBox.Text.ToLowerInvariant();
        firstNameTextBox.Text = Helper.FilterName(firstNameTextBox.Text);
        lastNameTextBox.Text = Helper.FilterName(lastNameTextBox.Text);

        bool hasError = false;

        if (!Helper.IsValidEmail(emailTextBox.Text)) {
            errorProvider.SetError(emailLabel, "Insert a valid email");
            hasError = true;
        }
        if (passwordTextBox.Text == "" && _bufferEditUser == null) {
            errorProvider.SetError(passwordLabel, "Insert a password");
            hasError = true;
        }
        if (!Helper.IsValidNif(nifMaskedTextBox.Text)) {
            errorProvider.SetError(nifLabel, "Insert a valid NIF");
            hasError = true;
        }
        var role = roleComboBox.SelectedItem?.ToString();
        if (role == null) {
            errorProvider.SetError(roleLabel, "Select the role");
            hasError = true;
        }
        if (!Helper.IsValidFirstName(firstNameTextBox.Text)) {
            errorProvider.SetError(firstNameLabel, "Insert a valid name");
            hasError = true;
        }
        if (!Helper.IsValidLastName(lastNameTextBox.Text)) {
            errorProvider.SetError(lastNameLabel, "Insert a valid name");
            hasError = true;
        }
        if (photoPictureBox.Image == null) {
            MessageBox.Show(
                "The user must have a photo!",
                "NO PHOTO",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            hasError = true;
        }

        using var dbContext = new EamDbContext();

        bool isRepeatedIdentification = dbContext.Users
            .Where(u => !u.IsDeleted)
            .Any(u => u.Identification == identificationTextBox.Text);
        if (isRepeatedIdentification) {
            errorProvider.SetError(identificationLabel, "This identification is already being used");
            hasError = true;
        }

        bool isRepeatedNif = dbContext.Users
            .Where(u => !u.IsDeleted)
            .Any(u => u.Nif == nifMaskedTextBox.Text);
        if (isRepeatedNif) {
            errorProvider.SetError(nifLabel, "This nif is already being used");
            hasError = true;
        }

        bool isRepeatedEmail = dbContext.Users
            .Where(u => !u.IsDeleted)
            .Any(u => u.Email == emailTextBox.Text);
        if (isRepeatedNif) {
            errorProvider.SetError(emailLabel, "This email is already being used");
            hasError = true;
        }

        var dbRole = dbContext.Roles.FirstOrDefault(r => r.Name == role);
        if (dbRole == null) {
            errorProvider.SetError(roleLabel, "Insert a valid role");
            hasError = true;
        }
        var @class = classComboBox.SelectedItem?.ToString();
        var dbClass = dbContext.Classes
            .Where(c => !c.IsDeleted)
            .Include(c => c.Course)
                .ThenInclude(c => c.Subjects)
                    .ThenInclude(s => s.Modules)
            .FirstOrDefault(c => c.Name == @class);
        if (@class != null && dbClass == null) {
            errorProvider.SetError(classOrSubjectsCountLabel, "Insert a valid class");
            hasError = true;
        }

        if (hasError) {
            return;
        }

        bool isSure = MessageBox.Show(
            "Are you sure you want to proceed?",
            "ARE YOU SURE ABOUT THAT?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        ) == DialogResult.Yes;

        if (!isSure) {
            return;
        }

        DialogResult result = DialogResult.OK;

        if (@class == null && role == "Student" && _bufferEditUser == null) {
            result = MessageBox.Show(
                "You are about to create a student without assigning a class." +
                "The student will be added but the class will be undefined until is manually added.\n" +
                "Are you sure you want to proceed?",
                "UNDEFINED CLASS",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
        }

        if (result == DialogResult.No) {
            return;
        }

        var newUser = new User {
            Identification = identificationTextBox.Text,
            Nif = nifMaskedTextBox.Text,
            FirstName = firstNameTextBox.Text,
            LastName = lastNameTextBox.Text,
            ProfilePic = Helper.ImageToByteArray(photoPictureBox.Image!),
            BirthDate = DateOnly.FromDateTime(birthDateDateTimePicker.Value),
            Role = dbRole!,
            RoleId = dbRole!.Id,
            Class = dbClass,
            ClassId = dbClass?.Id,
            Email = emailTextBox.Text,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordTextBox.Text),
            Subjects = [.. dbContext.Subjects
                .AsEnumerable()
                .Where(s => _bufferAddedSubjects.Any(_s => s.Id == _s.Id))
            ],
            IsDeleted = false,
        };

        if (newUser == null) {
            result = MessageBox.Show(
                "There was some error creating the user!",
                "ERROR CREATING THE USER",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Retry) {
                userConfirmButton_Click(sender, e);
            }
            return;
        }

        if (_bufferEditUser == null) {
            dbContext.Add(newUser);
        } else {
            User? editUser = dbContext.Users
                .Include(u => u.Class)
                .Include(u => u.Subjects)
                    .ThenInclude(s => s.Modules)
                        .ThenInclude(s => s.Requests)
                .Include(u => u.RequestStudents)
                .Include(u => u.RequestTeachers)
                .FirstOrDefault(u => u.Id == _bufferEditUser.Id)!;

            if (editUser == null) {
                DialogResult _result = MessageBox.Show(
                    "There was some error editing the user!",
                    "ERROR EDITING THE USER",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error
                );

                if (_result == DialogResult.Retry) {
                    userConfirmButton_Click(sender, e);
                }
                return;
            }
            if (newUser.RoleId == StudentRoleId) {
                var currentClass = dbContext.Classes
                    .Include(c => c.Course)
                        .ThenInclude(c => c.Subjects)
                            .ThenInclude(s => s.Modules)
                                .ThenInclude(m => m.Requests)
                    .FirstOrDefault(c => c.Id == editUser.ClassId);
                // ficou sem turma
                if (currentClass != null && newUser.Class == null) {
                    DialogResult _result = MessageBox.Show(
                        "Removing a class from the student will cancel all their pending requests!\n" +
                        "Are you sure you want to proceed?",
                        "ARE YOU SURE?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (_result == DialogResult.No) {
                        return;
                    }

                    foreach (Request request in editUser.RequestStudents) {
                        bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                               || request.StatusId == NotApprovedStatusId
                                               || request.StatusId == CanceledStatusId;

                        if (!isRequestCompleted) {
                            request.StatusId = CanceledStatusId;
                            var requestHistory = new RequestHistory {
                                Datetime = DateTime.Now,
                                RequestId = request.Id,
                                StatusId = CanceledStatusId,
                                UserId = editUser.Id,
                            };
                            dbContext.RequestHistories.Add(requestHistory);
                            dbContext.Update(request);
                        }
                    }
                }
                // mudou de curso
                if (currentClass != null && newUser.Class != null && currentClass.CourseId != newUser.Class.CourseId) {
                    DialogResult _result = MessageBox.Show(
                        "Changing the student's class will cancel all their pending requests that are not from the new course!\n" +
                        "Are you sure you want to proceed?",
                        "ARE YOU SURE?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (_result == DialogResult.No) {
                        return;
                    }

                    var removedSubjects = currentClass.Course.Subjects
                        .Where(s => !newUser.Class.Course.Subjects.Any(_s => _s.Id == s.Id))
                        ?? [];

                    foreach (Subject subject in removedSubjects) {
                        foreach (Module module in subject.Modules) {
                            foreach (Request request in module.Requests) {
                                bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                                        || request.StatusId == NotApprovedStatusId
                                                        || request.StatusId == CanceledStatusId;

                                if (!isRequestCompleted && request.StudentId == editUser.Id) {
                                    request.StatusId = CanceledStatusId;
                                    var requestHistory = new RequestHistory {
                                        Datetime = DateTime.Now,
                                        RequestId = request.Id,
                                        StatusId = CanceledStatusId,
                                        UserId = editUser.Id,
                                    };
                                    dbContext.RequestHistories.Add(requestHistory);
                                    dbContext.Update(request);
                                }
                            }
                        }
                    }
                }
                // passou de professor para aluno
                if (editUser.RoleId == TeacherRoleId) {
                    DialogResult _result = MessageBox.Show(
                        "Changing from teacher to student will cancel all their pending requests!\n" +
                        "Are you sure you want to proceed?",
                        "ARE YOU SURE?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (_result == DialogResult.No) {
                        return;
                    }

                    foreach (Request request in editUser.RequestTeachers) {
                        bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                                || request.StatusId == NotApprovedStatusId
                                                || request.StatusId == CanceledStatusId;

                        if (!isRequestCompleted) {
                            request.StatusId = CanceledStatusId;
                            var requestHistory = new RequestHistory {
                                Datetime = DateTime.Now,
                                RequestId = request.Id,
                                StatusId = CanceledStatusId,
                                UserId = editUser.Id,
                            };
                            dbContext.RequestHistories.Add(requestHistory);
                            dbContext.Update(request);
                        }
                    }
                }
            } else if (newUser.RoleId == TeacherRoleId) {
                // continua como professor (verificar se alguma disciplina foi removida)
                if (editUser.RoleId == TeacherRoleId) {
                    var removedSubjects = editUser.Subjects
                        .Where(s => !newUser.Subjects.Any(_s => _s.Id == s.Id))
                        ?? [];
                    if (removedSubjects.Any()) {
                        DialogResult _result = MessageBox.Show(
                            "Removing a subject from the teacher will cancel all pending requests associated with the teacher!\n" +
                            "Are you sure you want to proceed?",
                            "ARE YOU SURE?",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (_result == DialogResult.No) {
                            return;
                        }

                        foreach (Subject subject in removedSubjects) {
                            foreach (Module module in subject.Modules) {
                                foreach (Request request in module.Requests) {
                                    bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                                            || request.StatusId == NotApprovedStatusId
                                                            || request.StatusId == CanceledStatusId;

                                    if (!isRequestCompleted && request.TeacherId == editUser.Id) {
                                        request.StatusId = CanceledStatusId;
                                        var requestHistory = new RequestHistory {
                                            Datetime = DateTime.Now,
                                            RequestId = request.Id,
                                            StatusId = CanceledStatusId,
                                            UserId = editUser.Id,
                                        };
                                        dbContext.RequestHistories.Add(requestHistory);
                                        dbContext.Update(request);
                                    }
                                }
                            }
                        }
                    }
                }
                // passou de aluno para professor
                if (editUser.RoleId == StudentRoleId) {
                    DialogResult _result = MessageBox.Show(
                        "Changing from student to teacher will cancel all their pending requests!\n" +
                        "Are you sure you want to proceed?",
                        "ARE YOU SURE?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (_result == DialogResult.No) {
                        return;
                    }

                    foreach (Request request in editUser.RequestStudents) {
                        bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                                || request.StatusId == NotApprovedStatusId
                                                || request.StatusId == CanceledStatusId;

                        if (!isRequestCompleted) {
                            request.StatusId = CanceledStatusId;
                            var requestHistory = new RequestHistory {
                                Datetime = DateTime.Now,
                                RequestId = request.Id,
                                StatusId = CanceledStatusId,
                                UserId = editUser.Id,
                            };
                            dbContext.RequestHistories.Add(requestHistory);
                            dbContext.Update(request);
                        }
                    }
                }
            } else if (newUser.RoleId == SecretaryRoleId) {
                // passou de professor para secretario
                if (editUser.RoleId == TeacherRoleId) {
                    DialogResult _result = MessageBox.Show(
                        "Changing from teacher to secretary will cancel all their pending requests!\n" +
                        "Are you sure you want to proceed?",
                        "ARE YOU SURE?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (_result == DialogResult.No) {
                        return;
                    }

                    foreach (Request request in editUser.RequestTeachers) {
                        bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                                || request.StatusId == NotApprovedStatusId
                                                || request.StatusId == CanceledStatusId;

                        if (!isRequestCompleted) {
                            request.StatusId = CanceledStatusId;
                            var requestHistory = new RequestHistory {
                                Datetime = DateTime.Now,
                                RequestId = request.Id,
                                StatusId = CanceledStatusId,
                                UserId = editUser.Id,
                            };
                            dbContext.RequestHistories.Add(requestHistory);
                            dbContext.Update(request);
                        }
                    }
                }
                if (editUser.RoleId == StudentRoleId) {
                    DialogResult _result = MessageBox.Show(
                        "Changing from student to secretary will cancel all their pending requests!\n" +
                        "Are you sure you want to proceed?",
                        "ARE YOU SURE?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (_result == DialogResult.No) {
                        return;
                    }

                    foreach (Request request in editUser.RequestStudents) {
                        bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                               || request.StatusId == NotApprovedStatusId
                                               || request.StatusId == CanceledStatusId;

                        if (!isRequestCompleted) {
                            request.StatusId = CanceledStatusId;
                            var requestHistory = new RequestHistory {
                                Datetime = DateTime.Now,
                                RequestId = request.Id,
                                StatusId = CanceledStatusId,
                                UserId = editUser.Id,
                            };
                            dbContext.RequestHistories.Add(requestHistory);
                            dbContext.Update(request);
                        }
                    }
                }
            }

            editUser.Nif = newUser.Nif;
            editUser.FirstName = newUser.FirstName;
            editUser.LastName = newUser.LastName;
            editUser.ProfilePic = newUser.ProfilePic;
            editUser.BirthDate = newUser.BirthDate;
            editUser.Role = newUser.Role;
            editUser.Class = newUser.Class;
            editUser.ClassId = newUser.ClassId;
            editUser.Email = newUser.Email;
            editUser.PasswordHash = passwordTextBox.Text == "" ?
                editUser.PasswordHash
                : newUser.PasswordHash;
            editUser.Subjects = newUser.Subjects;

            dbContext.Update(editUser);
        }

        dbContext.SaveChanges();

        string action = _bufferEditUser == null ? "created" : "edited";

        ClearTabPage(userCreationTabPage);

        MessageBox.Show(
            $"User {action}!",
            "SUCESSFUL",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );

        if (action == "edited") {
            mainTabControl.SelectedTab = spectatorTabPage;
        }
    }

    private void photoPictureBox_Click(object sender, EventArgs e) {
        openFileDialog.FileName = "";
        openFileDialog.ShowDialog();
    }

    private void openFileDialog_FileOk(object sender, CancelEventArgs e) {
        string filePath = openFileDialog.FileName;

        bool isValidExtension = new List<string> {
            ".jpg", ".jpeg", ".jpe",
            ".jif", ".jfif", ".jfi",
        }.Contains(Path.GetExtension(filePath).ToLower());

        if (!isValidExtension) {
            MessageBox.Show(
                "Invalid file extension! Use JPEG files.",
                "INVALID FILE EXTENSION",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }
        photoPictureBox.Image = Helper.GetStretchedImage(filePath, 400, 400);
    }

    private void roleComboBox_SelectedIndexChanged(object sender, EventArgs e) {
        if (roleComboBox.SelectedItem == null) {
            classComboBox.Items.Clear();
            _bufferAddedSubjects.Clear();
            userSubjectsListView.Visible = false;
            classComboBox.Visible = false;
            classOrSubjectsCountLabel.Visible = false;
            return;
        }
        string selectedRole = roleComboBox.SelectedItem.ToString()!;
        classOrSubjectsCountLabel.Visible = selectedRole == "Student" || selectedRole == "Teacher";
        classComboBox.Visible = selectedRole == "Student";
        userSubjectsListView.Visible = selectedRole == "Teacher";

        if (selectedRole == "Student") {
            _bufferAddedSubjects.Clear();
            classOrSubjectsCountLabel.Text = "Class";
        } else if (selectedRole == "Teacher") {
            classComboBox.Items.Clear();
            classOrSubjectsCountLabel.Text = $"Subjects ({_bufferAddedSubjects.Count})";

            using var dbContext = new EamDbContext();

            IEnumerable<Subject> subjects = dbContext.Subjects
                .Where(s => !s.IsDeleted)
                .Include(s => s.Modules);

            IEnumerable<(ColumnHeader, Expression<Func<Subject, object>>)> subject_selectors = [
                ( new ColumnHeader { Text = "Abbreviation", Width = 90, TextAlign = HorizontalAlignment.Left }
                , s => s.Abbreviation ),
                ( new ColumnHeader { Text = "Name", Width = 300, TextAlign = HorizontalAlignment.Left }
                , s => s.Name ),
                ( new ColumnHeader { Text = "Modules", Width = 70, TextAlign = HorizontalAlignment.Right }
                , s => s.Modules.Count ),
                ( new ColumnHeader { Text = "Duration", Width = 90, TextAlign = HorizontalAlignment.Right }
                , s => IntToTimeString(s.Modules.Sum(m => m.DurationMin)) ),
            ];

            userSubjectsListView.ItemChecked -= userSubjectsListView_ItemChecked;
            Helper.InjectToListView(userSubjectsListView, subjects, subject_selectors);
            userSubjectsListView.ItemChecked += userSubjectsListView_ItemChecked;

            for (int i = 0; i < subjects.Count(); i++) {
                userSubjectsListView.Items[i].Tag = subjects.ElementAt(i).Id;
            }

            for (int i = 0; i < subjects.Count(); i++) {
                int id = (int)userSubjectsListView.Items[i].Tag!;
                if (_bufferAddedSubjects.Any(s => s.Id == id)) {
                    userSubjectsListView.ItemChecked -= userSubjectsListView_ItemChecked;
                    userSubjectsListView.Items[i].Checked = true;
                    userSubjectsListView.ItemChecked += userSubjectsListView_ItemChecked;
                }
            }
        }
    }

    private void addModuleButton_Click(object sender, EventArgs e) {
        int moduleNumber = (int)moduleNumberNumericUpDown.Value;
        moduleNameTextBox.Text = Helper.FilterName(moduleNameTextBox.Text);
        string moduleName = moduleNameTextBox.Text;
        int? moduleDuration = (int?)TimeSpanFromString(moduleDurationMaskedTextBox.Text)?.TotalMinutes;

        addModuleButton.Text = "Add";

        if (moduleDuration == null) {
            DialogResult result = MessageBox.Show(
                "There was some error in duration!",
                "ERROR IN DURATION",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Retry) {
                addModuleButton_Click(sender, e);
            }
            return;
        }

        int hours = (int)moduleDuration / 60;
        int minutes = (int)moduleDuration % 60;

        var item = _bufferEditModule == null ?
            new ListViewItem()
            : subjectModulesListView.Items
                .Cast<ListViewItem>()
                .FirstOrDefault(i =>
                    _bufferEditModule.Number.ToString() == i.SubItems[0].Text
                );

        if (item == null) {
            _bufferEditModule = null;
            return;
        }

        item.SubItems.Clear();
        item.Text = moduleNumber.ToString();
        item.SubItems.AddRange([
            moduleName,
            $"{hours}h {minutes}m",
        ]);

        if (_bufferEditModule == null) {
            _bufferAddedModules.Add(new Module {
                Number = moduleNumber,
                Name = moduleName,
                DurationMin = (int)moduleDuration,
            });
            subjectModulesListView.Items.Add(item);
        } else {
            _bufferEditModule.Number = moduleNumber;
            _bufferEditModule.Name = moduleName;
            _bufferEditModule.DurationMin = (int)moduleDuration;
        }

        moduleNameTextBox.Text = "";
        moduleDurationMaskedTextBox.Text = "";
        subjectModulesListView.BackColor = Color.White;

        bool isRepeatedModuleNumber(int n) => subjectModulesListView.Items
            .Cast<ListViewItem>()
            .Any(i => i.SubItems[0].Text == n.ToString());

        moduleNumberNumericUpDown.Value = 1;
        while (isRepeatedModuleNumber((int)moduleNumberNumericUpDown.Value)) {
            moduleNumberNumericUpDown.Value++;
        }
        _bufferEditModule = null;

        subjectModulesCountLabel.Text = $"Modules ({_bufferAddedModules.Count})";
    }

    private void subjectConfirmButton_Click(object sender, EventArgs e) {
        using var dbContext = new EamDbContext();

        errorProvider.Clear();

        bool hasError = false;

        subjectAbbreviationTextBox.Text = subjectAbbreviationTextBox.Text.Trim().ToUpper();

        if (!Helper.IsValidAbbreviation(subjectAbbreviationTextBox.Text)) {
            hasError = true;
            errorProvider.SetError(subjectAbbreviationLabel, "Insert a valid abbreviation");
        }

        subjectNameTextBox.Text = Helper.FilterName(subjectNameTextBox.Text);

        if (subjectNameTextBox.Text == "") {
            hasError = true;
            errorProvider.SetError(subjectNameLabel, "Insert a valid name");
        }

        bool isRepeatedName = dbContext.Subjects
            .Where(s => !s.IsDeleted)
            .Any(s =>
                s.Name == subjectNameTextBox.Text
                && (_bufferEditSubject == null || s.Name != _bufferEditSubject.Name)
            );
        if (isRepeatedName) {
            hasError = true;
            errorProvider.SetError(subjectNameLabel, "Name already being used");
        }

        bool isRepeatedAbbvreviation = dbContext.Subjects
            .Where(s => !s.IsDeleted)
            .Any(s =>
                s.Abbreviation == subjectAbbreviationTextBox.Text
                && (_bufferEditSubject == null || s.Abbreviation != _bufferEditSubject.Abbreviation)
            );
        if (isRepeatedAbbvreviation) {
            hasError = true;
            errorProvider.SetError(subjectAbbreviationLabel, "Abbreviation already being used");
        }

        if (hasError) {
            return;
        }

        bool isSure = MessageBox.Show(
            "Are you sure you want to proceed?",
            "ARE YOU SURE ABOUT THAT?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        ) == DialogResult.Yes;

        if (!isSure) {
            return;
        }

        DialogResult result = DialogResult.OK;
        if (_bufferAddedTeachers.Count == 0) {
            result = MessageBox.Show(
                "You are about to create a subject without assigning any teachers." +
                "The subject will be created, but it will not have any teachers assigned until one is manually added.\n" +
                "Are you sure you want to proceed?",
                "NO TEACHERS ASSIGNED",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
        }

        if (result == DialogResult.No) {
            return;
        }
        var newSubject = new Subject {
            Abbreviation = subjectAbbreviationTextBox.Text,
            Name = subjectNameTextBox.Text,
            Modules = [.. _bufferAddedModules],
            Teachers = [.. dbContext.Users
                .AsEnumerable()
                .Where(u => u.RoleId == TeacherRoleId)
                .Where(t => _bufferAddedTeachers.Any(_t => t.Id == _t.Id))
            ]
        };

        if (newSubject == null) {
            result = MessageBox.Show(
                "There was some error creating the subject!",
                "ERROR CREATING THE SUBJECT",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Retry) {
                subjectConfirmButton_Click(sender, e);
            }
            return;
        }

        if (_bufferEditSubject == null) {
            dbContext.Add(newSubject);
        } else {
            Subject? editSubject = dbContext.Subjects
                .Include(s => s.Teachers)
                    .ThenInclude(t => t.RequestTeachers)
                        .ThenInclude(r => r.Module)
                .Include(s => s.Modules)
                    .ThenInclude(m => m.Requests)
                .FirstOrDefault(s => s.Id == _bufferEditSubject.Id)!;

            if (editSubject == null) {
                DialogResult _result = MessageBox.Show(
                    "There was some error editing the subject!",
                    "ERROR EDITING THE SUBJECT",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error
                );

                if (_result == DialogResult.Retry) {
                    subjectConfirmButton_Click(sender, e);
                }
                return;
            }

            var removedTeachers = editSubject.Teachers
                .Where(t => !newSubject.Teachers.Any(_t => _t.Id == t.Id));

            bool isOkWithRemovingTeachers = false;

            if (removedTeachers.Any()) {
                result = MessageBox.Show(
                    "Removing a teacher from a subject will cancel all associated pending requests!\n" +
                    "Are you sure you want to proceed?",
                    "ARE YOU SURE?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No) {
                    return;
                }
                isOkWithRemovingTeachers = true;
            }

            var removedModules = editSubject.Modules
                .Where(m => !newSubject.Modules.Any(_m => _m.Id == m.Id));

            bool isOkWithRemovingModules = false;

            if (removedModules.Any()) {
                result = MessageBox.Show(
                    "Removing a module from a subject will cancel all associated pending requests!\n" +
                    "Are you sure you want to proceed?",
                    "ARE YOU SURE?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No) {
                    return;
                }
                isOkWithRemovingModules = true;
            }

            if (isOkWithRemovingTeachers) {
                foreach (User teacher in removedTeachers) {
                    foreach (Request request in teacher.RequestTeachers) {
                        if (request.Module.SubjectId != editSubject.Id) {
                            continue;
                        }

                        bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                                || request.StatusId == NotApprovedStatusId
                                                || request.StatusId == CanceledStatusId;
                        if (isRequestCompleted) {
                            continue;
                        }

                        request.StatusId = CanceledStatusId;
                        var requestHistory = new RequestHistory {
                            Datetime = DateTime.Now,
                            RequestId = request.Id,
                            StatusId = CanceledStatusId,
                            UserId = teacher.Id,
                        };
                        dbContext.RequestHistories.Add(requestHistory);
                        dbContext.Update(request);
                    }
                }
            }
            if (isOkWithRemovingModules) {
                // "remover" os modulos removidos
                foreach (Module module in editSubject.Modules) {
                    if (newSubject.Modules.Any(m => m.Id == module.Id)) {
                        continue;
                    }
                    foreach (Request request in module.Requests) {
                        bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                               || request.StatusId == NotApprovedStatusId
                                               || request.StatusId == CanceledStatusId;
                        if (!isRequestCompleted) {
                            request.StatusId = CanceledStatusId;
                            var requestHistory = new RequestHistory {
                                Datetime = DateTime.Now,
                                RequestId = request.Id,
                                StatusId = CanceledStatusId,
                                UserId = request.Teacher.Id,
                            };
                            dbContext.RequestHistories.Add(requestHistory);
                            dbContext.Update(request);
                        }
                    }
                    module.IsDeleted = true;
                    dbContext.Update(module);
                }
            }

            editSubject.Abbreviation = newSubject.Abbreviation;
            editSubject.Name = newSubject.Name;
            editSubject.Teachers = newSubject.Teachers;

            // atualizar os modulos presentes
            foreach (Module module in editSubject.Modules) {
                var _module = newSubject.Modules.FirstOrDefault(m => m.Id == module.Id);

                if (_module != null) {
                    module.Number = _module.Number;
                    module.Name = _module.Name;
                    module.DurationMin = _module.DurationMin;
                    dbContext.Update(module);
                }
            }
            // adiocionar os modulos adicionados
            foreach (Module module in newSubject.Modules) {
                if (!editSubject.Modules.Any(m => m.Id == module.Id)) {
                    editSubject.Modules.Add(module);
                }
            }
            dbContext.Update(editSubject);
            dbContext.SaveChanges();

            string action = _bufferEditSubject == null ? "created" : "edited";

            ClearTabPage(subjectCreationTabPage);

            MessageBox.Show(
                $"Subject {action}!",
                "SUCESSFUL",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            if (action == "edited") {
                mainTabControl.SelectedTab = spectatorTabPage;
            }
        }
    }

    private void moduleNumberNumericUpDown_ValueChanged(object sender, EventArgs e) {
        addModuleButton.Enabled = IsValidModuleInput();
    }

    private void moduleNameTextBox_TextChanged(object sender, EventArgs e) {
        addModuleButton.Enabled = IsValidModuleInput();
    }

    private void moduleDurationMaskedTextBox_TextChanged(object sender, EventArgs e) {
        addModuleButton.Enabled = IsValidModuleInput();
    }

    private bool IsValidModuleInput() {
        int moduleNumber = (int)moduleNumberNumericUpDown.Value;
        string moduleName = moduleNameTextBox.Text;
        TimeSpan? moduleDuration = TimeSpanFromString(moduleDurationMaskedTextBox.Text);

        bool isRepeatedModuleNumber = subjectModulesListView.Items
            .Cast<ListViewItem>()
            .Any(i => i.SubItems[0].Text == moduleNumber.ToString()
                   && moduleNumber != _bufferEditModule?.Number
            );

        return moduleDuration != null
            && Helper.FilterName(moduleName) != ""
            && !isRepeatedModuleNumber;
    }

    public static TimeSpan? TimeSpanFromString(string str) {
        try {
            int[] time = str                                       // "00h 00m"
                .Split(' ', StringSplitOptions.RemoveEmptyEntries) // ["00h", "00m"] 
                .Select(x => new string([.. x.SkipLast(1)]))       // ["00", "00"]
                .Select(x => Convert.ToInt32(x))                   // [00, 00]
                .ToArray();

            if (time[1] >= 60) {
                return null;
            }

            return new TimeSpan(time[0], time[1], 0);
        } catch {
            return null;
        };
    }

    public static string IntToTimeString(int minutes) {
        int totalHours = minutes / 60;
        minutes %= 60;

        return $"{totalHours}h {minutes}m";
    }

    private void subjectModulesListView_KeyDown(object sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Delete) {
            for (int i = 0; i < subjectModulesListView.Items.Count; i++) {
                if (subjectModulesListView.Items[i].Selected) {
                    subjectModulesListView.Items.RemoveAt(i);
                    _bufferAddedModules.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    private void deleteModuleToolStripMenuItem_Click(object sender, EventArgs e) {
        for (int i = 0; i < subjectModulesListView.Items.Count; i++) {
            if (subjectModulesListView.Items[i].Selected) {
                subjectModulesListView.Items.RemoveAt(i);
                _bufferAddedModules.RemoveAt(i);
            }
        }
    }

    private void editModuleToolStripMenuItem_Click(object sender, EventArgs e) {
        if (_bufferPoint == null) {
            return;
        }
        ListViewItem? item = subjectModulesListView.GetItemAt(_bufferPoint.Value.x, _bufferPoint.Value.y);
        if (item == null) {
            return;
        }
        foreach (ListViewItem i in subjectModulesListView.SelectedItems) {
            if (i != item) {
                i.Selected = false;
            }
        }
        _bufferEditModule = _bufferAddedModules.FirstOrDefault(m =>
            m.Number.ToString() == item.SubItems[0].Text
        );
        if (_bufferEditModule == null) {
            return;
        }

        subjectModulesListView.BackColor = Color.Gainsboro;

        moduleNumberNumericUpDown.Value = Convert.ToDecimal(item.SubItems[0].Text);
        moduleNameTextBox.Text = item.SubItems[1].Text;

        string[] parts = item.SubItems[2].Text // "00h 00m"
            .Split(' ');                       // ["00h", "00m"]
        string strHours = parts[0].Replace("h", "");
        string strMinutes = parts[1].Replace("m", "");

        moduleDurationMaskedTextBox.Text = $"{strHours,3}h {strMinutes,2}m";

        addModuleButton.Text = "Confirm";
    }

    private void courseConfirmButton_Click(object sender, EventArgs e) {
        using var dbContext = new EamDbContext();

        errorProvider.Clear();

        bool hasError = false;

        courseAbbreviationTextBox.Text = courseAbbreviationTextBox.Text.Trim().ToUpper();

        if (!Helper.IsValidAbbreviation(courseAbbreviationTextBox.Text)) {
            hasError = true;
            errorProvider.SetError(courseAbbreviationLabel, "Insert a valid abbreviation");
        }

        courseNameTextBox.Text = Helper.FilterName(courseNameTextBox.Text);

        if (courseNameTextBox.Text == "") {
            hasError = true;
            errorProvider.SetError(courseNameLabel, "Insert a valid name");
        }

        bool isRepeatedName = dbContext.Courses
            .Where(c => c.IsDeleted)
            .Any(c => c.Name == courseNameTextBox.Text
                   && (_bufferEditCourse == null || c.Name != _bufferEditCourse.Name)
            );

        if (isRepeatedName) {
            hasError = true;
            errorProvider.SetError(courseNameLabel, "Name already being used");
        }

        bool isRepeatedAbbreviation = dbContext.Courses
            .Where(c => c.IsDeleted)
            .Any(c => c.Abbreviation == courseAbbreviationTextBox.Text
                   && (_bufferEditCourse == null || c.Abbreviation != _bufferEditCourse.Abbreviation)
            );

        if (isRepeatedAbbreviation) {
            hasError = true;
            errorProvider.SetError(courseAbbreviationLabel, "Abbreviation already being used");
        }

        if (hasError) {
            return;
        }

        bool isSure = MessageBox.Show(
            "Are you sure you want to proceed?",
            "ARE YOU SURE ABOUT THAT?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        ) == DialogResult.Yes;

        if (!isSure) {
            return;
        }

        DialogResult result;

        if (!_bufferAddedSubjects.Any()) {
            result = MessageBox.Show(
                "You are about to create a course without subjects.\n" +
                "The subject will be created, but it will not have any subjects assigned until one is manually added.\n" +
                "Are you sure you want to proceed?",
                "NO SUBJECTS",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
        }

        var newCourse = new Course {
            Abbreviation = courseAbbreviationTextBox.Text,
            Name = courseNameTextBox.Text,
            Subjects = [.. dbContext.Subjects
                .AsEnumerable()
                .Where(s => _bufferAddedSubjects.Any(bs => s.Id == bs.Id))
            ],
        };

        if (newCourse == null) {
            result = MessageBox.Show(
                "There was some error creating the course!",
                "ERROR CREATING THE COURSE",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Retry) {
                courseConfirmButton_Click(sender, e);
            }
            return;
        }

        if (_bufferEditCourse == null) {
            dbContext.Add(newCourse);
        } else {
            var editCourse = dbContext.Courses
                .Include(c => c.Subjects)
                    .ThenInclude(s => s.Modules)
                        .ThenInclude(m => m.Requests)
                .FirstOrDefault(c => c.Id == _bufferEditCourse.Id);

            if (editCourse == null) {
                result = MessageBox.Show(
                    "There was some error editing the course!",
                    "ERROR EDITING THE COURSE",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error
                );

                if (result == DialogResult.Retry) {
                    classConfirmButton_Click(sender, e);
                }
                return;
            }

            var removedSubjects = editCourse.Subjects
                .Where(s => !newCourse.Subjects.Any(_s => _s.Id == s.Id));

            if (removedSubjects.Any()) {
                result = MessageBox.Show(
                    "Removing a subject from a course will cancel all associated pending requests!\n" +
                    "Are you sure you want to proceed?",
                    "ARE YOU SURE?",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No) {
                    return;
                }

                var courseStudents = dbContext.Users
                    .Where(u => u.RoleId == StudentRoleId)
                    .Where(s => s.Class != null)
                    .Include(s => s.RequestStudents)
                    .Include(s => s.Class)
                        .ThenInclude(c => c!.Course)
                            .ThenInclude(c => c.Subjects)
                                .ThenInclude(s => s.Modules)
                    .Where(s => s.Class!.CourseId == editCourse.Id);

                foreach (User student in courseStudents) {
                    foreach (Request request in student.RequestStudents) {
                        if (!removedSubjects.Any(s => s.Id == request.Module.Subject.Id)) {
                            continue;
                        }

                        bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                               || request.StatusId == NotApprovedStatusId
                                               || request.StatusId == CanceledStatusId;
                        if (isRequestCompleted) {
                            continue;
                        }

                        request.StatusId = CanceledStatusId;
                        var requestHistory = new RequestHistory {
                            Datetime = DateTime.Now,
                            RequestId = request.Id,
                            StatusId = CanceledStatusId,
                            UserId = request.TeacherId,
                        };
                        dbContext.RequestHistories.Add(requestHistory);
                        dbContext.Update(request);
                    }
                }
            }

            editCourse.Abbreviation = newCourse.Abbreviation;
            editCourse.Name = newCourse.Name;
            editCourse.Subjects = newCourse.Subjects;

            dbContext.Update(editCourse);
        }
        dbContext.SaveChanges();

        string action = _bufferEditCourse == null ? "created" : "edited";

        ClearTabPage(courseCreationTabPage);

        MessageBox.Show(
            $"Course {action}!",
            "SUCESSFUL",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );

        if (action == "edited") {
            mainTabControl.SelectedTab = spectatorTabPage;
        }
    }

    private void courseSubjectsListView_ItemChecked(object sender, ItemCheckedEventArgs e) {
        ListViewItem checkedItem = e.Item;

        using var dbContext = new EamDbContext();

        Subject? subject = dbContext.Subjects
            .Where(s => !s.IsDeleted)
            .FirstOrDefault(s => s.Id == (int)checkedItem.Tag!);

        if (subject == null) {
            DialogResult result = MessageBox.Show(
                "There was some error selecting the subject!",
                "ERROR SELECTING THE SUBJECT",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Retry) {
                courseSubjectsListView_ItemChecked(sender, e);
            }
            courseSubjectsListView.ItemChecked -= courseSubjectsListView_ItemChecked;
            checkedItem.Checked = false;
            courseSubjectsListView.ItemChecked += courseSubjectsListView_ItemChecked;
            return;
        }

        if (checkedItem.Checked) {
            _bufferAddedSubjects.Add(subject!);
        } else {
            _bufferAddedSubjects.Remove(
                _bufferAddedSubjects.FirstOrDefault(s => s.Id == subject.Id) ?? new Subject()
            );
        }
        courseSubjectsCountLabel.Text = $"Subjects ({_bufferAddedSubjects.Count})";
    }

    private void classCourseComboBox_SelectedIndexChanged(object sender, EventArgs e) {
        using var dbContext = new EamDbContext();

        if (classCourseComboBox.SelectedItem == null) {
            classSubjectsReadOnlyListView.Columns.Clear();
            classSubjectsReadOnlyListView.Items.Clear();
            return;
        }

        string courseName = classCourseComboBox.SelectedItem.ToString()!;

        Course? course = dbContext.Courses
            .Where(c => !c.IsDeleted)
            .FirstOrDefault(c => c.Name == courseName);

        if (course == null) {
            /* TODO msg de erro */
            return;
        }

        IEnumerable<Subject> subjects = dbContext.Subjects
            .Where(s => s.Courses.Contains(course))
            .Where(s => !s.IsDeleted)
            .Include(s => s.Modules);

        IEnumerable<(ColumnHeader, Expression<Func<Subject, object>>)> selectors = [
            ( new ColumnHeader { Text = "Abbreviation", Width = 80, TextAlign = HorizontalAlignment.Left }
            , s => s.Abbreviation ),
            ( new ColumnHeader { Text = "Name", Width = 200, TextAlign = HorizontalAlignment.Left }
            , s => s.Name ),
            ( new ColumnHeader { Text = "Modules", Width = 70, TextAlign = HorizontalAlignment.Right }
            , s => s.Modules.Count ),
            ( new ColumnHeader { Text = "Duration", Width = 90, TextAlign = HorizontalAlignment.Right }
            , s => IntToTimeString(s.Modules.Sum(m => m.DurationMin)) ),
        ];

        Helper.InjectToListView(classSubjectsReadOnlyListView, subjects, selectors);
    }

    private void classCourseComboBox_DropDown(object sender, EventArgs e) {
        string? selected = classCourseComboBox.SelectedItem?.ToString();

        using var dbContext = new EamDbContext();
        Helper.InjectToComboBox(classCourseComboBox, dbContext.Courses.Where(c => !c.IsDeleted), c => c.Name);

        classCourseComboBox.SelectedItem = selected;
    }

    private void classConfirmButton_Click(object sender, EventArgs e) {
        using var dbContext = new EamDbContext();

        errorProvider.Clear();

        bool hasError = false;

        classNameTextBox.Text = Helper.FilterName(classNameTextBox.Text);

        if (classNameTextBox.Text == "") {
            errorProvider.SetError(classNameLabel, "Insert a valid name");
            hasError = true;
        }

        if (classCourseComboBox.SelectedItem == null) {
            errorProvider.SetError(classCourseLabel, "Select the course");
            hasError = true;
        }

        if (hasError) {
            return;
        }

        bool isSure = MessageBox.Show(
            "Are you sure you want to proceed?",
            "ARE YOU SURE ABOUT THAT?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        ) == DialogResult.Yes;

        if (!isSure) {
            return;
        }

        DialogResult result = DialogResult.OK;

        if (_bufferAddedStudents.Count == 0 && _bufferEditClass == null) {
            result = MessageBox.Show(
                "You are about to create a class without students.\n" +
                "Are you sure you want to proceed?",
                "NO STUDENTS",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
        }

        if (result == DialogResult.No) {
            return;
        }

        var course = dbContext.Courses
            .Where(c => !c.IsDeleted)
            .Include(c => c.Subjects)
                .ThenInclude(s => s.Modules)
                    .ThenInclude(m => m.Requests)
            .AsEnumerable()
            .FirstOrDefault(c => c.Name == classCourseComboBox.SelectedItem!.ToString());

        if (course == null) {
            result = MessageBox.Show(
                "There was some error selecting the course!",
                "ERROR SELECTING THE COURSE",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Retry) {
                classConfirmButton_Click(sender, e);
            }
            return;
        }

        var studentsToSwitch = _bufferAddedStudents
            .Where(s => s.ClassId != null && s.ClassId != _bufferEditClass?.Id);

        if (studentsToSwitch.Any()) {
            result = MessageBox.Show(
                "You are assignin students that already have a class!\n" +
                "Changing a student's class will cancel all their pending requests that are not from the new course!\n" +
                "Are you sure you want to proceed?",
                "ARE YOU SURE?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.No) {
                return;
            }
        }

        var newClass = new Class {
            Name = classNameTextBox.Text,
            MaxStudents = (int)classMaxStudentsNumericUpDown.Value,
            Users = [.. dbContext.Users
                .AsEnumerable()
                .Where(s => _bufferAddedStudents.Any(bs => s.Id == bs.Id))
            ],
            Course = course,
            CourseId = course.Id,
        };

        if (newClass == null) {
            // TODO
            return;
        }

        if (_bufferEditClass == null) {
            dbContext.Add(newClass);
        } else {
            var editClass = dbContext.Classes
                .Include(c => c.Course)
                    .ThenInclude(c => c.Subjects)
                        .ThenInclude(s => s.Modules)
                            .ThenInclude(m => m.Requests)
                .Include(c => c.Users)
                    .ThenInclude(s => s.RequestStudents)
                .FirstOrDefault(c => c.Id == _bufferEditClass.Id);

            if (editClass == null) {
                result = MessageBox.Show(
                    "There was some error editing the class!",
                    "ERROR EDITING THE CLASS",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error
                );

                if (result == DialogResult.Retry) {
                    classConfirmButton_Click(sender, e);
                }
                return;
            }

            var removedStudents = editClass.Users
                .Where(s => !newClass.Users.Any(_s => _s.Id == s.Id));

            if (removedStudents.Any()) {
                result = MessageBox.Show(
                    "You are removing students from the class.\n" +
                    "Removing the class from the student will make them unusable and cancel all associated pending requests\n" +
                    "The student will have the class undefined until is manually added.\n" +
                    "Are you sure you want to proceed?",
                    "UNDEFINED CLASS",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No) {
                    return;
                }

                foreach (User student in removedStudents) {
                    foreach (Request request in student.RequestStudents) {
                        bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                               || request.StatusId == NotApprovedStatusId
                                               || request.StatusId == CanceledStatusId;

                        if (!isRequestCompleted) {
                            request.StatusId = CanceledStatusId;
                            var requestHistory = new RequestHistory {
                                Datetime = DateTime.Now,
                                RequestId = request.Id,
                                StatusId = CanceledStatusId,
                                UserId = student.Id,
                            };
                            dbContext.RequestHistories.Add(requestHistory);
                            dbContext.Update(request);
                        }
                    }
                }
            }

            editClass.Name = newClass.Name;
            editClass.MaxStudents = newClass.MaxStudents;
            editClass.Users = newClass.Users;

            dbContext.Update(editClass);
        }

        foreach (User student in studentsToSwitch) {
            var currentStudentClass = dbContext.Classes
                .Include(c => c.Course)
                    .ThenInclude(c => c.Subjects)
                        .ThenInclude(s => s.Modules)
                            .ThenInclude(m => m.Requests)
                .FirstOrDefault(c => c.Id == student.ClassId);

            if (currentStudentClass == null) {
                continue;
            }

            if (currentStudentClass.CourseId != newClass.CourseId) {
                var removedSubjects = currentStudentClass.Course.Subjects
                    .Where(s => !newClass.Course.Subjects.Any(_s => _s.Id == s.Id))
                    ?? [];

                foreach (Subject subject in removedSubjects) {
                    foreach (Module module in subject.Modules) {
                        foreach (Request request in module.Requests) {
                            bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                                   || request.StatusId == NotApprovedStatusId
                                                   || request.StatusId == CanceledStatusId;

                            if (!isRequestCompleted && request.StudentId == student.Id) {
                                request.StatusId = CanceledStatusId;
                                var requestHistory = new RequestHistory {
                                    Datetime = DateTime.Now,
                                    RequestId = request.Id,
                                    StatusId = CanceledStatusId,
                                    UserId = student.Id,
                                };
                                dbContext.RequestHistories.Add(requestHistory);
                                dbContext.Update(request);
                            }
                        }
                    }
                }
            }
        }

        dbContext.SaveChanges();

        string action = _bufferEditClass == null ? "created" : "edited";

        ClearTabPage(classCreationTabPage);

        MessageBox.Show(
            $"Class {action}!",
            "SUCESSFUL",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );

        if (action == "edited") {
            mainTabControl.SelectedTab = spectatorTabPage;
        }
    }

    private void classStudentsListView_ItemChecked(object sender, ItemCheckedEventArgs e) {
        ListViewItem checkedItem = e.Item;

        using var dbContext = new EamDbContext();

        User? student = dbContext.Users
            .AsEnumerable()
            .FirstOrDefault(s => s.Id == (int)checkedItem.Tag!
                              && s.RoleId == StudentRoleId
                              && !s.IsDeleted
            );

        if (student == null) {
            DialogResult result = MessageBox.Show(
                "There was some error selecting the student!",
                "ERROR SELECTING THE STUDENT",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Retry) {
                courseSubjectsListView_ItemChecked(sender, e);
            }
            classStudentsListView.ItemChecked -= classStudentsListView_ItemChecked;
            checkedItem.Checked = false;
            classStudentsListView.ItemChecked += classStudentsListView_ItemChecked;
            return;
        }

        if (checkedItem.Checked) {
            _bufferAddedStudents.Add(student!);
        } else {
            _bufferAddedStudents.Remove(
                _bufferAddedStudents.FirstOrDefault(s => s.Id == student.Id) ?? new User()
            );
        }
        classStudentsCountLabel.Text = $"Students ({_bufferAddedStudents.Count})";
    }

    private void classStudentsListView_ItemCheck(object sender, ItemCheckEventArgs e) {
        ListViewItem checkedItem = classStudentsListView.Items[e.Index];

        if (e.NewValue == CheckState.Checked && _bufferAddedStudents.Count == classMaxStudentsNumericUpDown.Value) {
            e.NewValue = CheckState.Unchecked;
        }
    }

    private void spectatorComboBox_DropDown(object sender, EventArgs e) {
        string? selected = spectatorComboBox.SelectedItem?.ToString();

        using var dbContext = new EamDbContext();

        string[] options = [
            "User",
            .. dbContext.Roles.Select(r => r.Name),
            "Class",
            "Course",
            "Subject",
            "Module",
            "Situation",
            "Request",
        ];
        Helper.InjectToComboBox(spectatorComboBox, options);
        spectatorComboBox.SelectedItem = selected;
    }

    private void spectatorComboBox_SelectedIndexChanged(object sender, EventArgs e) {
        if (spectatorComboBox.SelectedItem == null) {
            spectatorListView.Items.Clear();
            spectatorListView.Columns.Clear();
            return;
        }

        using var dbContext = new EamDbContext();

        string option = spectatorComboBox.SelectedItem.ToString()!;
        switch (option) {
        case "User":
            IEnumerable<User> users = dbContext.Users
                .Include(u => u.Role)
                .OrderDescending();

            IEnumerable<(ColumnHeader, Expression<Func<User, object>>)> userSelectors = [
                ( new ColumnHeader { Text = "Id", Width = 70, TextAlign = HorizontalAlignment.Right }
                , s => s.Identification ),
                ( new ColumnHeader { Text = "First Name", Width = 150, TextAlign = HorizontalAlignment.Left }
                , s => s.FirstName ),
                ( new ColumnHeader { Text = "Last Name", Width = 200, TextAlign = HorizontalAlignment.Left }
                , s => s.LastName ),
                ( new ColumnHeader { Text = "Email", Width = 200, TextAlign = HorizontalAlignment.Left }
                , s => s.Email ),
                ( new ColumnHeader { Text = "Role", Width = 80, TextAlign = HorizontalAlignment.Left }
                , s => s.Role.Name ),
                ( new ColumnHeader { Text = "State", Width = 80, TextAlign = HorizontalAlignment.Left }
                , s => s.IsDeleted ? "Deleted" : "Active" ),
            ];
            Helper.InjectToListView(spectatorListView, users, userSelectors);
            for (int i = 0; i < users.Count(); i++) {
                spectatorListView.Items[i].Tag = users.ElementAt(i).Id;
            }
            break;

        case "Student":
            IEnumerable<User> students = dbContext.Users
                .Where(u => u.RoleId == StudentRoleId)
                .Include(s => s.Class)
                .Include(s => s.RequestStudents)
                .OrderDescending();

            IEnumerable<(ColumnHeader, Expression<Func<User, object>>)> studentSelectors = [
                ( new ColumnHeader { Text = "Id", Width = 60, TextAlign = HorizontalAlignment.Right }
                , s => s.Identification ),
                ( new ColumnHeader { Text = "First Name", Width = 150, TextAlign = HorizontalAlignment.Left }
                , s => s.FirstName ),
                ( new ColumnHeader { Text = "Last Name", Width = 180, TextAlign = HorizontalAlignment.Left }
                , s => s.LastName ),
                ( new ColumnHeader { Text = "Email", Width = 180, TextAlign = HorizontalAlignment.Left }
                , s => s.Email ),
                ( new ColumnHeader { Text = "Class", Width = 80, TextAlign = HorizontalAlignment.Left }
                , s => s.Class == null ? "Undefined" : s.Class.Name ),
                ( new ColumnHeader { Text = "Requests", Width = 80, TextAlign = HorizontalAlignment.Right }
                , s => s.RequestStudents.Count ),
                ( new ColumnHeader { Text = "State", Width = 80, TextAlign = HorizontalAlignment.Left }
                , s => s.IsDeleted ? "Deleted" : "Active" ),
            ];
            Helper.InjectToListView(spectatorListView, students, studentSelectors);
            for (int i = 0; i < students.Count(); i++) {
                spectatorListView.Items[i].Tag = students.ElementAt(i).Id;
            }
            break;

        case "Teacher":
            IEnumerable<User> teachers = dbContext.Users
                .Where(u => u.RoleId == TeacherRoleId)
                .Include(t => t.Subjects)
                .Include(t => t.RequestTeachers)
                .OrderDescending();

            IEnumerable<(ColumnHeader, Expression<Func<User, object>>)> teacherSelectors = [
                ( new ColumnHeader { Text = "Id", Width = 60, TextAlign = HorizontalAlignment.Right }
                , s => s.Identification ),
                ( new ColumnHeader { Text = "First Name", Width = 150, TextAlign = HorizontalAlignment.Left }
                , s => s.FirstName ),
                ( new ColumnHeader { Text = "Last Name", Width = 180, TextAlign = HorizontalAlignment.Left }
                , s => s.LastName ),
                ( new ColumnHeader { Text = "Email", Width = 180, TextAlign = HorizontalAlignment.Left }
                , s => s.Email ),
                ( new ColumnHeader { Text = "Subjects", Width = 80, TextAlign = HorizontalAlignment.Right }
                , s => s.Subjects.Where(s => !s.IsDeleted).Count() ),
                ( new ColumnHeader { Text = "Requests", Width = 80, TextAlign = HorizontalAlignment.Right }
                , s => s.RequestTeachers.Count ),
                ( new ColumnHeader { Text = "State", Width = 80, TextAlign = HorizontalAlignment.Left }
                , s => s.IsDeleted ? "Deleted" : "Active" ),
            ];
            Helper.InjectToListView(spectatorListView, teachers, teacherSelectors);
            for (int i = 0; i < teachers.Count(); i++) {
                spectatorListView.Items[i].Tag = teachers.ElementAt(i).Id;
            }
            break;

        case "Secretary":
            IEnumerable<User> secretaries = dbContext.Users
                .Where(u => u.RoleId == SecretaryRoleId)
                .Include(s => s.Subjects)
                .Include(s => s.RequestHistories)
                .OrderDescending();

            IEnumerable<(ColumnHeader, Expression<Func<User, object>>)> secretarySelectors = [
                ( new ColumnHeader { Text = "Id", Width = 60, TextAlign = HorizontalAlignment.Right }
                , s => s.Identification ),
                ( new ColumnHeader { Text = "First Name", Width = 150, TextAlign = HorizontalAlignment.Left }
                , s => s.FirstName ),
                ( new ColumnHeader { Text = "Last Name", Width = 180, TextAlign = HorizontalAlignment.Left }
                , s => s.LastName ),
                ( new ColumnHeader { Text = "Email", Width = 180, TextAlign = HorizontalAlignment.Left }
                , s => s.Email ),
                ( new ColumnHeader { Text = "Requests", Width = 80, TextAlign = HorizontalAlignment.Left }
                , s => s.RequestHistories.Count ),
                ( new ColumnHeader { Text = "State", Width = 80, TextAlign = HorizontalAlignment.Left }
                , s => s.IsDeleted ? "Deleted" : "Active" ),
            ];
            Helper.InjectToListView(spectatorListView, secretaries, secretarySelectors);
            for (int i = 0; i < secretaries.Count(); i++) {
                spectatorListView.Items[i].Tag = secretaries.ElementAt(i).Id;
            }
            break;

        case "Class":
            IEnumerable<Class> classes = dbContext.Classes
                .Include(c => c.Users)
                .Include(c => c.Course)
                .OrderDescending();

            IEnumerable<(ColumnHeader, Expression<Func<Class, object>>)> classSelectors = [
                ( new ColumnHeader { Text = "Name", Width = 300, TextAlign = HorizontalAlignment.Left }
                , c => c.Name ),
                ( new ColumnHeader { Text = "Course", Width = 200, TextAlign = HorizontalAlignment.Left }
                , c => c.Course.Name ),
                ( new ColumnHeader { Text = "Students", Width = 100, TextAlign = HorizontalAlignment.Right }
                , c => c.Users.Where(s => !s.IsDeleted).Count() ),
                ( new ColumnHeader { Text = "Max Students", Width = 100, TextAlign = HorizontalAlignment.Right }
                , c => c.MaxStudents ),
                ( new ColumnHeader { Text = "Satus", Width = 100, TextAlign = HorizontalAlignment.Right }
                , c => c.IsDeleted ? "Deleted" : "Available" ),
            ];
            Helper.InjectToListView(spectatorListView, classes, classSelectors);
            for (int i = 0; i < classes.Count(); i++) {
                spectatorListView.Items[i].Tag = classes.ElementAt(i).Id;
            }
            break;

        case "Course":
            IEnumerable<Course> courses = dbContext.Courses
                .Include(c => c.Subjects)
                .OrderDescending();

            IEnumerable<(ColumnHeader, Expression<Func<Course, object>>)> courseSelectors = [
                ( new ColumnHeader { Text = "Abbreviation", Width = 100, TextAlign = HorizontalAlignment.Left }
                , c => c.Abbreviation ),
                ( new ColumnHeader { Text = "Name", Width = 500, TextAlign = HorizontalAlignment.Left }
                , c => c.Name ),
                ( new ColumnHeader { Text = "Subjects", Width = 100, TextAlign = HorizontalAlignment.Right }
                , c => c.Subjects.Where(s => !s.IsDeleted).Count() ),
                ( new ColumnHeader { Text = "Satus", Width = 100, TextAlign = HorizontalAlignment.Right }
                , c => c.IsDeleted ? "Deleted" : "Available" ),
            ];
            Helper.InjectToListView(spectatorListView, courses, courseSelectors);
            for (int i = 0; i < courses.Count(); i++) {
                spectatorListView.Items[i].Tag = courses.ElementAt(i).Id;
            }
            break;

        case "Subject":
            IEnumerable<Subject> subjects = dbContext.Subjects
                .Include(s => s.Modules)
                .Include(s => s.Teachers)
                .OrderDescending();

            IEnumerable<(ColumnHeader, Expression<Func<Subject, object>>)> subjectSelectors = [
                ( new ColumnHeader { Text = "Abbreviation", Width = 100, TextAlign = HorizontalAlignment.Left }
                , s => s.Abbreviation ),
                ( new ColumnHeader { Text = "Name", Width = 300, TextAlign = HorizontalAlignment.Left }
                , s => s.Name ),
                ( new ColumnHeader { Text = "Modules", Width = 100, TextAlign = HorizontalAlignment.Right }
                , s => s.Modules.Where(m => !m.IsDeleted).Count() ),
                ( new ColumnHeader { Text = "Teachers", Width = 100, TextAlign = HorizontalAlignment.Right }
                , s => s.Teachers.Where(t => !t.IsDeleted).Count() ),
                ( new ColumnHeader { Text = "Satus", Width = 100, TextAlign = HorizontalAlignment.Right }
                , s => s.IsDeleted ? "Deleted" : "Available" ),
            ];
            Helper.InjectToListView(spectatorListView, subjects, subjectSelectors);
            for (int i = 0; i < subjects.Count(); i++) {
                spectatorListView.Items[i].Tag = subjects.ElementAt(i).Id;
            }
            break;

        case "Module":
            IEnumerable<Module> modules = dbContext.Modules
                .Include(m => m.Subject)
                .Include(m => m.Requests)
                .OrderDescending();

            IEnumerable<(ColumnHeader, Expression<Func<Module, object>>)> moduleSelectors = [
                ( new ColumnHeader { Text = "Number", Width = 80, TextAlign = HorizontalAlignment.Right }
                , m => m.Number ),
                ( new ColumnHeader { Text = "Name", Width = 200, TextAlign = HorizontalAlignment.Left }
                , m => m.Name ),
                ( new ColumnHeader { Text = "Subject", Width = 150, TextAlign = HorizontalAlignment.Left }
                , m => m.Subject.Name ),
                ( new ColumnHeader { Text = "Duration", Width = 90, TextAlign = HorizontalAlignment.Right }
                , m => IntToTimeString(m.DurationMin) ),
                ( new ColumnHeader { Text = "Requests", Width = 80, TextAlign = HorizontalAlignment.Right }
                , m => m.Requests.Count ),
                ( new ColumnHeader { Text = "Satus", Width = 100, TextAlign = HorizontalAlignment.Left }
                , m => m.IsDeleted ? "Deleted" : "Available" ),
            ];
            Helper.InjectToListView(spectatorListView, modules, moduleSelectors);
            for (int i = 0; i < modules.Count(); i++) {
                spectatorListView.Items[i].Tag = modules.ElementAt(i).Id;
            }
            break;

        case "Situation":
            IEnumerable<Situation> situations = dbContext.Situations
                .Include(s => s.Requests)
                .OrderDescending();

            IEnumerable<(ColumnHeader, Expression<Func<Situation, object>>)> situationSelectors = [
                ( new ColumnHeader { Text = "Name", Width = 200, TextAlign = HorizontalAlignment.Left }
                , s => s.Name ),
                ( new ColumnHeader { Text = "Start", Width = 100, TextAlign = HorizontalAlignment.Right }
                , s => s.StartAt == null ? "-- / --" : s.StartAt.Value.ToString("dd / MM") ),
                ( new ColumnHeader { Text = "Subject", Width = 100, TextAlign = HorizontalAlignment.Right }
                , s => s.EndAt == null ? "-- / --" : s.EndAt.Value.ToString("dd / MM") ),
                ( new ColumnHeader { Text = "Tax", Width = 100, TextAlign = HorizontalAlignment.Right }
                , s => s.Tax.ToString("0.00") + "€" ),
                ( new ColumnHeader { Text = "Requests", Width = 80, TextAlign = HorizontalAlignment.Right }
                , s => s.Requests.Count ),
                ( new ColumnHeader { Text = "Status", Width = 100, TextAlign = HorizontalAlignment.Left }
                , s => s.IsDeleted ? "Deleted" : "Available" ),
            ];
            Helper.InjectToListView(spectatorListView, situations, situationSelectors);
            for (int i = 0; i < situations.Count(); i++) {
                spectatorListView.Items[i].Tag = situations.ElementAt(i).Id;
            }
            break;

        case "Request":
            IEnumerable<Request> requests = dbContext.Requests
                .Include(r => r.Teacher)
                .Include(r => r.Student)
                .Include(r => r.Status)
                .Include(r => r.Module)
                    .ThenInclude(m => m.Subject)
                        .ThenInclude(m => m.Courses)
                .Include(r => r.RequestHistories)
                    .ThenInclude(rh => rh.User)
                .OrderDescending();
            if (_bufferUserRequests != null) {
                requests = requests
                    .Where(r => r.StudentId == _bufferUserRequests.Id
                             || r.TeacherId == _bufferUserRequests.Id
                             || r.RequestHistories.Any(rh => rh.UserId == _bufferUserRequests.Id)
                    );
            }
            if (_bufferStudentRequests != null) {
                requests = requests
                    .Where(r => r.StudentId == _bufferStudentRequests.Id);
            } else if (_bufferTeacherRequests != null) {
                requests = requests
                    .Where(r => r.TeacherId == _bufferTeacherRequests.Id);
            }
            if (_bufferSecretaryRequests != null) {
                requests = requests
                    .Where(r => r.RequestHistories.Any(rh => rh.UserId == _bufferSecretaryRequests.Id));
            }
            if (_bufferModuleRequests != null) {
                requests = requests
                    .Where(r => r.ModuleId == _bufferModuleRequests.Id);
            }
            if (_bufferSubjectRequests != null) {
                requests = requests
                    .Where(r => r.Module.SubjectId == _bufferSubjectRequests.Id);
            }
            if (_bufferCourseRequests != null) {
                requests = requests
                    .Where(r => r.Module.Subject.Courses.Any(c => c.Id == _bufferCourseRequests.Id));
            }

            IEnumerable<(ColumnHeader, Expression<Func<Request, object>>)> requestSelectors = [
                ( new ColumnHeader { Text = "Number", Width = 80, TextAlign = HorizontalAlignment.Right }
                , r => r.Number ),
                ( new ColumnHeader { Text = "Module", Width = 100, TextAlign = HorizontalAlignment.Left }
                , r => $"{r.Module.Number} - {r.Module.Name}" ),
                ( new ColumnHeader { Text = "Subject", Width = 100, TextAlign = HorizontalAlignment.Left }
                , r => $"{r.Module.Subject.Abbreviation} - {r.Module.Subject.Name}" ),
                ( new ColumnHeader { Text = "Student", Width = 120, TextAlign = HorizontalAlignment.Left }
                , r => $"{r.Student.Identification} - {r.Student.FirstName} {r.Student.LastName}" ),
                ( new ColumnHeader { Text = "Teacher", Width = 120, TextAlign = HorizontalAlignment.Left }
                , r => $"{r.Teacher.Identification} - {r.Teacher.FirstName} {r.Teacher.LastName}" ),
                ( new ColumnHeader { Text = "Duration", Width = 70, TextAlign = HorizontalAlignment.Right }
                , r => IntToTimeString(r.DurationMin) ),
                ( new ColumnHeader { Text = "Status", Width = 100, TextAlign = HorizontalAlignment.Left }
                , r => r.Status.Description ),
                ( new ColumnHeader { Text = "Payment", Width = 100, TextAlign = HorizontalAlignment.Left }
                , r => r.PaymentMethod == null ? "" : r.PaymentMethod ),
                ( new ColumnHeader { Text = "Grade", Width = 100, TextAlign = HorizontalAlignment.Left }
                , r => r.Grade == null ? "" : $"{r.Grade:00.00} / 20" ),
            ];
            Helper.InjectToListView(spectatorListView, requests, requestSelectors);
            for (int i = 0; i < requests.Count(); i++) {
                spectatorListView.Items[i].Tag = requests.ElementAt(i).Id;
            }
            break;
        }

        for (int i = 0; i < spectatorListView.Items.Count; i++) {
            ListViewItem item = spectatorListView.Items[i];
            bool isValid = string.IsNullOrWhiteSpace(spectatorSearchBarTextBox.Text)
                || spectatorSearchBarTextBox.Text
                    .Split([' '], StringSplitOptions.RemoveEmptyEntries)  // Split the input into words
                    .All(word => item.SubItems
                        .Cast<ListViewItem.ListViewSubItem>()
                        .Any(s => s.Text.Contains(word, spectatorCaseSensitiveCheckBox.Checked ?
                            StringComparison.OrdinalIgnoreCase
                            : StringComparison.CurrentCulture
                        ))
                    );

            if (!isValid) {
                spectatorListView.Items.Remove(item);
                i--;
            }
        }
    }

    private void showAllStudentsCheckBox_CheckedChanged(object sender, EventArgs e) {
        using var dbContext = new EamDbContext();

        IEnumerable<User> students = dbContext.Users
            .Where(u => !u.IsDeleted)
            .Where(u => u.RoleId == StudentRoleId)
            .Where(u => classShowAllStudentsCheckBox.Checked ?
                u.Class == u.Class
                : u.Class == null
            )
            .Include(u => u.Class);

        IEnumerable<(ColumnHeader, Expression<Func<User, object>>)> student_selectors = [
            ( new ColumnHeader { Text = "Id", Width = 80, TextAlign = HorizontalAlignment.Right }
            , s => s.Identification ),
            ( new ColumnHeader { Text = "First Name", Width = 90, TextAlign = HorizontalAlignment.Left }
            , s => s.FirstName ),
            ( new ColumnHeader { Text = "Last Name", Width = 90, TextAlign = HorizontalAlignment.Left }
            , s => s.LastName ),
            ( new ColumnHeader { Text = "Email", Width = 90, TextAlign = HorizontalAlignment.Left }
            , s => s.Email ),
            ( new ColumnHeader { Text = "Class", Width = 90, TextAlign = HorizontalAlignment.Left }
            , s => s.Class == null ? "Undefined" : s.Class.Name ),
        ];

        classStudentsListView.ItemChecked -= classStudentsListView_ItemChecked;
        Helper.InjectToListView(classStudentsListView, students, student_selectors);
        classStudentsListView.ItemChecked += classStudentsListView_ItemChecked;

        for (int i = 0; i < students.Count(); i++) {
            classStudentsListView.Items[i].Tag = students.ElementAt(i).Id;
        }

        for (int i = 0; i < students.Count(); i++) {
            int id = (int)classStudentsListView.Items[i].Tag!;
            if (_bufferAddedStudents.Any(s => s.Id == id)) {
                classStudentsListView.ItemChecked -= classStudentsListView_ItemChecked;
                classStudentsListView.Items[i].Checked = true;
                classStudentsListView.ItemChecked += classStudentsListView_ItemChecked;
            }
        }
    }

    private void subjectModulesListView_MouseDown(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            ListViewItem? item = subjectModulesListView.GetItemAt(e.X, e.Y);
            refreshModulesToolStripMenuItem.Visible = item == null;
            editModuleToolStripMenuItem.Visible = item != null;
            deleteModuleToolStripMenuItem.Visible = item != null;
            _bufferPoint = (e.X, e.Y);
            modulesContextMenuStrip.Show(subjectModulesListView, e.Location);
        }
    }

    private void subjectTeachersListView_ItemChecked(object sender, ItemCheckedEventArgs e) {
        ListViewItem checkedItem = e.Item;

        using var dbContext = new EamDbContext();

        User? teacher = dbContext.Users
            .Where(t => t.RoleId == TeacherRoleId)
            .FirstOrDefault(t => t.Identification == checkedItem.SubItems[0].Text);

        if (teacher == null) {
            DialogResult result = MessageBox.Show(
                "There was some error selecting the teacher!",
                "ERROR SELECTING THE TEACHER",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Retry) {
                subjectTeachersListView_ItemChecked(sender, e);
            }
            subjectTeachersListView.ItemChecked -= subjectTeachersListView_ItemChecked;
            checkedItem.Checked = false;
            subjectTeachersListView.ItemChecked += subjectTeachersListView_ItemChecked;
            return;
        }

        if (checkedItem.Checked) {
            _bufferAddedTeachers.Add(teacher!);
        } else {
            _bufferAddedTeachers.Remove(
                _bufferAddedTeachers.FirstOrDefault(t => t.Identification == teacher.Identification) ?? new User()
            );
        }
        courseSubjectsCountLabel.Text = $"Teachers ({_bufferAddedTeachers.Count})";
    }

    private void userSubjectsListView_ItemChecked(object sender, ItemCheckedEventArgs e) {
        ListViewItem checkedItem = e.Item;

        using var dbContext = new EamDbContext();

        Subject? subject = dbContext.Subjects
            .AsEnumerable()
            .FirstOrDefault(t => t.Id == (int)checkedItem.Tag!);

        if (subject == null) {
            DialogResult result = MessageBox.Show(
                "There was some error selecting the subject!",
                "ERROR SELECTING THE SUBJECT",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Retry) {
                userSubjectsListView_ItemChecked(sender, e);
            }
            userSubjectsListView.ItemChecked -= userSubjectsListView_ItemChecked;
            checkedItem.Checked = false;
            userSubjectsListView.ItemChecked += userSubjectsListView_ItemChecked;
            return;
        }

        if (checkedItem.Checked) {
            _bufferAddedSubjects.Add(subject!);
        } else {
            _bufferAddedSubjects.Remove(
                _bufferAddedSubjects.FirstOrDefault(t => t.Name == subject.Name) ?? new Subject()
            );
        }
        classOrSubjectsCountLabel.Text = $"Subjects ({_bufferAddedSubjects.Count})";
    }

    private void refreshModulesToolStripMenuItem_Click(object sender, EventArgs e) {
        subjectModulesListView.Items.Clear();

        foreach (Module module in _bufferAddedModules) {
            int hours = module.DurationMin / 60;
            int minutes = module.DurationMin % 60;

            ListViewItem item = new ListViewItem(module.Number.ToString());
            item.SubItems.AddRange([
                module.Name,
                $"{hours}h {minutes}m",
            ]);
            subjectModulesListView.Items.Add(item);
        }
    }

    private void refreshTeachersToolStripMenuItem_Click(object sender, EventArgs e) {
        using var dbContext = new EamDbContext();

        IEnumerable<User> teachers = dbContext.Users
            .Where(u => !u.IsDeleted)
            .Where(u => u.RoleId == TeacherRoleId)
            .Include(t => t.Subjects);

        IEnumerable<(ColumnHeader, Expression<Func<User, object>>)> teacher_selectors = [
            ( new ColumnHeader { Text = "ID", Width = 80, TextAlign = HorizontalAlignment.Right }
            , t => t.Identification ),
            ( new ColumnHeader { Text = "Name", Width = 200, TextAlign = HorizontalAlignment.Left }
            , t => t.FirstName + " " + t.LastName ),
            ( new ColumnHeader { Text = "Subjects", Width = 70, TextAlign = HorizontalAlignment.Right }
            , t => t.Subjects.Count ),
        ];

        subjectTeachersListView.ItemChecked -= subjectTeachersListView_ItemChecked;
        Helper.InjectToListView(subjectTeachersListView, teachers, teacher_selectors);
        foreach (ListViewItem item in subjectTeachersListView.Items) {
            bool isAdded = _bufferAddedTeachers.Any(t => t.Identification == item.SubItems[0].Text);
            if (isAdded) {
                item.Checked = true;
            }
        }
        subjectTeachersListView.ItemChecked += subjectTeachersListView_ItemChecked;
    }

    private void spectatorSearchBarTextBox_TextChanged(object sender, EventArgs e) {
        spectatorComboBox_SelectedIndexChanged(sender, e);
    }

    private void subjectTeachersListView_MouseDown(object sender, MouseEventArgs e) {

        if (e.Button == MouseButtons.Right) {
            ListViewItem? item = subjectTeachersListView.GetItemAt(e.X, e.Y);
            refreshTeachersToolStripMenuItem.Visible = item == null;
            checkTeacherToolStripMenuItem.Visible = item != null && item.Checked == false;
            uncheckTeacherToolStripMenuItem.Visible = item != null && item.Checked == true;
            _bufferPoint = (e.X, e.Y);
            teachersContextMenuStrip.Show(subjectTeachersListView, e.Location);
        }
    }

    private void spectatorListView_MouseDown(object sender, MouseEventArgs e) {
        if (spectatorComboBox.SelectedItem == null) {
            spectatorRefreshToolStripMenuItem.Visible = true;
            spectatorEditToolStripMenuItem.Visible = false;
            spectatorDeleteToolStripMenuItem.Visible = false;
            spectatorRequestsToolStripMenuItem.Visible = false;
            spectatorContextMenuStrip.Show(spectatorListView, e.Location);
            return;
        }
        if (e.Button == MouseButtons.Right) {
            string option = spectatorComboBox.SelectedItem.ToString()!;
            bool isUpdatable = option != "Request";
            bool displayableRequests = option == "User"
                                    || option == "Student"
                                    || option == "Teacher"
                                    || option == "Secretary" // TODO talvez
                                    || option == "Subject"
                                    || option == "Module"
                                    || option == "Course"; // TODO talvez
            bool hasFilter = option == "Request"
                          && (_bufferUserRequests != null
                             || _bufferStudentRequests != null
                             || _bufferTeacherRequests != null
                             || _bufferSecretaryRequests != null
                             || _bufferSubjectRequests != null
                             || _bufferModuleRequests != null
                             || _bufferCourseRequests != null
                             );
            ListViewItem? item = spectatorListView.GetItemAt(e.X, e.Y);
            spectatorRefreshToolStripMenuItem.Visible = item == null;
            spectatorEditToolStripMenuItem.Visible = item != null && isUpdatable;
            spectatorDeleteToolStripMenuItem.Visible = item != null && isUpdatable;
            spectatorRequestsToolStripMenuItem.Visible = item != null && displayableRequests;
            spectatorRemoveFilterToolStripMenuItem.Visible = item == null && hasFilter;
            _bufferPoint = (e.X, e.Y);
            spectatorContextMenuStrip.Show(spectatorListView, e.Location);
        }
    }

    private void checkTeacherToolStripMenuItem_Click(object sender, EventArgs e) {
        if (_bufferPoint == null) {
            return;
        }
        ListViewItem? item = subjectTeachersListView.GetItemAt(_bufferPoint.Value.x, _bufferPoint.Value.y);
        if (item == null) {
            return;
        }
        if (!item.Checked) {
            item.Checked = true;
        }
    }

    private void uncheckTeacherToolStripMenuItem_Click(object sender, EventArgs e) {
        if (_bufferPoint == null) {
            return;
        }
        ListViewItem? item = subjectTeachersListView.GetItemAt(_bufferPoint.Value.x, _bufferPoint.Value.y);
        if (item == null) {
            return;
        }
        if (item.Checked) {
            item.Checked = false;
        }
    }

    private void situationEnableDateCheckBox_CheckedChanged(object sender, EventArgs e) {
        situationStartDateLabel.Enabled = situationEnableDateCheckBox.Checked;
        startDateFormatLabel.Enabled = situationEnableDateCheckBox.Checked;
        situationStartDateMaskedTextBox.Enabled = situationEnableDateCheckBox.Checked;

        situationEndDateLabel.Enabled = situationEnableDateCheckBox.Checked;
        endDateFormatLabel.Enabled = situationEnableDateCheckBox.Checked;
        situationEndDateMaskedTextBox.Enabled = situationEnableDateCheckBox.Checked;
    }

    private void situationConfirmButton_Click(object sender, EventArgs e) {
        errorProvider.Clear();

        situationNameTextBox.Text = Helper.FilterName(situationNameTextBox.Text);

        bool hasError = false;

        using var dbContext = new EamDbContext();

        if (situationNameTextBox.Text == "") {
            errorProvider.SetError(situationNameLabel, "Insert a valid name");
            hasError = true;
        }

        bool isRepeatedName = dbContext.Situations.Any(s => s.Name == situationNameTextBox.Text
                                                         && (_bufferEditSituation == null || s.Name != _bufferEditSituation.Name)
        );

        if (isRepeatedName) {
            errorProvider.SetError(situationNameLabel, "This name is already being used");
            hasError = true;
        }

        if (situationEnableDateCheckBox.Checked && !situationStartDateMaskedTextBox.MaskCompleted) {
            errorProvider.SetError(situationStartDateLabel, "Insert a valid date");
            hasError = true;
        }
        if (situationEnableDateCheckBox.Checked && !situationEndDateMaskedTextBox.MaskCompleted) {
            errorProvider.SetError(situationEndDateLabel, "Insert a valid date");
            hasError = true;
        }

        bool areBothDatesCompleted = situationStartDateMaskedTextBox.MaskCompleted
                                  && situationEndDateMaskedTextBox.MaskCompleted;

        DateOnly startDate = default;
        DateOnly endDate = default;

        if (situationEnableDateCheckBox.Checked && areBothDatesCompleted) {
            int[] startDateArr = situationStartDateMaskedTextBox.Text // "90 / 90"
                .Split('/')                                           // ["90 ", "90 "]
                .Select(s => s.Trim())                                // ["90", "90"]
                .Select(s => Convert.ToInt32(s))                      // [90, 90]
                .ToArray();

            int[] endDateArr = situationEndDateMaskedTextBox.Text // "90 / 90"
                .Split('/')                                       // ["90 ", "90 "]
                .Select(s => s.Trim())                            // ["90", "90"]
                .Select(s => Convert.ToInt32(s))                  // [90, 90]
                .ToArray();

            bool isValidStart = DateOnly.TryParse($"2000-{startDateArr[1]:D2}-{startDateArr[0]:D2}", out startDate);
            bool isValidEnd = DateOnly.TryParse($"2000-{endDateArr[1]:D2}-{endDateArr[0]:D2}", out endDate);

            if (!isValidStart) {
                errorProvider.SetError(situationStartDateLabel, "Insert a valid date");
                hasError = true;
            }
            if (!isValidEnd) {
                errorProvider.SetError(situationEndDateLabel, "Insert a valid date");
                hasError = true;
            }

            if (isValidStart && isValidEnd) {
                if (endDate < startDate) {
                    errorProvider.SetError(situationStartDateLabel, "Start Date must be before End Date");
                    errorProvider.SetError(situationEndDateLabel, "End Date must be after Start Date");
                    hasError = true;
                } else {
                    Situation? mergedSituation = dbContext.Situations
                        .Where(s => s.StartAt.HasValue)
                        .FirstOrDefault(s => (s.StartAt! <= startDate && startDate <= s.EndAt!)
                                          || (s.StartAt! <= endDate && endDate <= s.EndAt!)
                        );

                    if (mergedSituation != null && mergedSituation.Name != _bufferEditSituation?.Name) {
                        errorProvider.SetError(situationEnableDateCheckBox, $"This situation is merging with \"{mergedSituation.Name}\"");
                        hasError = true;
                    }
                }
            }
        }

        if (hasError) {
            return;
        }

        bool isSure = MessageBox.Show(
            "Are you sure you want to proceed?",
            "ARE YOU SURE ABOUT THAT?",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        ) == DialogResult.Yes;

        if (!isSure) {
            return;
        }

        var situation = new Situation() {
            Name = situationNameTextBox.Text,
            StartAt = situationEnableDateCheckBox.Checked ? startDate : null,
            EndAt = situationEnableDateCheckBox.Checked ? endDate : null,
            Tax = situationTaxNumericUpDown.Value,
        };

        if (situation == null) {
            DialogResult result = MessageBox.Show(
                "There was some error creating the situation!",
                "ERROR CREATING THE SITUATION",
                MessageBoxButtons.RetryCancel,
                MessageBoxIcon.Error
            );

            if (result == DialogResult.Retry) {
                situationConfirmButton_Click(sender, e);
            }
            return;
        }

        if (_bufferEditSituation == null) {
            dbContext.Add(situation);
        } else {
            var editSituation = dbContext.Situations
                .FirstOrDefault(s => s.Id == _bufferEditSituation.Id);

            if (editSituation == null) {
                DialogResult result = MessageBox.Show(
                    "There was some error editing the situation!",
                    "ERROR EDITING THE SITUATION",
                    MessageBoxButtons.RetryCancel,
                    MessageBoxIcon.Error
                );

                if (result == DialogResult.Retry) {
                    situationConfirmButton_Click(sender, e);
                }
                return;
            }

            editSituation.Name = situation.Name;
            editSituation.StartAt = situation.StartAt;
            editSituation.EndAt = situation.EndAt;
            editSituation.Tax = situation.Tax;

            dbContext.Update(editSituation);
        }

        dbContext.SaveChanges();

        string action = _bufferEditSituation == null ? "created" : "edited";

        ClearTabPage(situationCreationTabPage);

        MessageBox.Show(
            $"Situation {action}!",
            "SUCESSFUL",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );

        if (action == "edited") {
            mainTabControl.SelectedTab = spectatorTabPage;
        }

    }

    private void spectatorEditToolStripMenuItem_Click(object sender, EventArgs e) {
        if (_bufferPoint == null) {
            return;
        }

        using var dbContext = new EamDbContext();

        void EditUser(User user) {
            mainTabControl.SelectedTab = userCreationTabPage;
            mainTabControl.SelectedTab.BackColor = Color.Gainsboro;

            userConfirmButton.Text = "Confirm (edit)";

            roleComboBox.SelectedItem = user.Role.Name;
            identificationTextBox.Enabled = false;
            identificationTextBox.Text = user.Identification;
            firstNameTextBox.Text = user.FirstName;
            lastNameTextBox.Text = user.LastName;
            nifMaskedTextBox.Text = user.Nif;
            emailTextBox.Text = user.Email;
            photoPictureBox.Image = Helper.ByteArrayToImage(user.ProfilePic);
            birthDateDateTimePicker.Value = user.BirthDate.ToDateTime(TimeOnly.MinValue);

            if (user.Role.Id == StudentRoleId) {
                Class? @class = dbContext.Classes.Find(user.ClassId)!;
                classComboBox.SelectedItem = @class?.Name;
            } else if (user.Role.Id == TeacherRoleId) {
                List<Subject> selectedSubjects = [.. user.Subjects
                    .Where(s => !s.IsDeleted)
                ];

                IEnumerable<Subject> dbSubjects = dbContext.Subjects
                    .Where(s => !s.IsDeleted)
                    .Include(s => s.Modules);

                IEnumerable<(ColumnHeader, Expression<Func<Subject, object>>)> subject_selectors = [
                    ( new ColumnHeader { Text = "Abbreviation", Width = 90, TextAlign = HorizontalAlignment.Left }
                    , s => s.Abbreviation ),
                    ( new ColumnHeader { Text = "Name", Width = 300, TextAlign = HorizontalAlignment.Left }
                    , s => s.Name ),
                    ( new ColumnHeader { Text = "Modules", Width = 70, TextAlign = HorizontalAlignment.Right }
                    , s => s.Modules.Count ),
                    ( new ColumnHeader { Text = "Duration", Width = 90, TextAlign = HorizontalAlignment.Right }
                    , s => IntToTimeString(s.Modules.Sum(m => m.DurationMin)) ),
                ];

                userSubjectsListView.ItemChecked -= userSubjectsListView_ItemChecked;
                Helper.InjectToListView(userSubjectsListView, dbSubjects, subject_selectors);
                userSubjectsListView.ItemChecked += userSubjectsListView_ItemChecked;

                for (int i = 0; i < dbSubjects.Count(); i++) {
                    userSubjectsListView.Items[i].Tag = dbSubjects.ElementAt(i).Id;
                }

                foreach (ListViewItem item in userSubjectsListView.Items) {
                    if (selectedSubjects.Any(s => s.Id == (int)item.Tag!)) {
                        item.Checked = true;
                    }
                }
            }
            _bufferEditUser = user;
        }
        void EditSituation(Situation situation) {
            if (situation.Requests.Count > 0) {
                MessageBox.Show(
                    "Can't edit a situation that is already being used!",
                    "SITUATION ALREADY BEING USED",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }
            mainTabControl.SelectedTab = situationCreationTabPage;
            mainTabControl.SelectedTab.BackColor = Color.Gainsboro;

            situationNameTextBox.Text = situation.Name;
            if (situation.StartAt != null) {
                situationEnableDateCheckBox.Checked = true;

                (int sDay, int sMonth) = (situation.StartAt.Value.Day, situation.StartAt.Value.Month);
                situationStartDateMaskedTextBox.Text = $"{sDay:D2} / {sMonth:D2}";

                (int eDay, int eMonth) = (situation.EndAt!.Value.Day, situation.EndAt!.Value.Month);
                situationEndDateMaskedTextBox.Text = $"{eDay:D2} / {eMonth:D2}";
            }
            situationTaxNumericUpDown.Value = situation.Tax;
            situationConfirmButton.Text = "Confirm (edit)";

            _bufferEditSituation = situation;
        }
        void EditClass(Class @class) {
            mainTabControl.SelectedTab = classCreationTabPage;
            mainTabControl.SelectedTab.BackColor = Color.Gainsboro;

            classNameTextBox.Text = @class.Name;
            classMaxStudentsNumericUpDown.Value = @class.MaxStudents;
            classCourseComboBox.DroppedDown = true;  // tem que abrir e fechar para
            classCourseComboBox.DroppedDown = false; // ativar o evento onde coloca os cursos
            classCourseComboBox.SelectedItem = @class.Course.Name; // para depois poder escolher por codigo
            classCourseComboBox.Enabled = false;
            classConfirmButton.Text = "Confirm (edit)";

            classShowAllStudentsCheckBox.Checked = true;

            List<User> selectedStudents = [.. @class.Users
                .Where(s => !s.IsDeleted)
            ];

            foreach (ListViewItem item in classStudentsListView.Items) {
                if (selectedStudents.Any(s => s.Id == (int)item.Tag!)) {
                    item.Checked = true;
                }
            }

            _bufferEditClass = @class;
        }
        void EditCourse(Course course) {
            mainTabControl.SelectedTab = courseCreationTabPage;
            mainTabControl.SelectedTab.BackColor = Color.Gainsboro;

            courseAbbreviationTextBox.Text = course.Abbreviation;
            courseNameTextBox.Text = course.Name;

            courseConfirmButton.Text = "Confirm (edit)";

            List<Subject> selectedSubjects = [.. course.Subjects
                .Where(s => !s.IsDeleted)
            ];

            foreach (ListViewItem item in courseSubjectsListView.Items) {
                if (selectedSubjects.Any(s => s.Id == (int)item.Tag!)) {
                    item.Checked = true;
                }
            }

            _bufferEditCourse = course;
        }
        void EditSubject(Subject subject) {
            mainTabControl.SelectedTab = subjectCreationTabPage;
            mainTabControl.SelectedTab.BackColor = Color.Gainsboro;

            subjectAbbreviationTextBox.Text = subject.Abbreviation;
            subjectNameTextBox.Text = subject.Name;

            subjectModulesListView.Items.Clear();
            foreach (Module module in subject.Modules) {
                int hours = module.DurationMin / 60;
                int minutes = module.DurationMin % 60;

                var item = new ListViewItem(module.Number.ToString()) {
                    Tag = module.Id,
                };
                item.SubItems.AddRange([
                    module.Name,
                    $"{hours}h {minutes}m",
                ]);
                subjectModulesListView.Items.Add(item);
                _bufferAddedModules.Add(module);
            }

            List<User> selectedTeachers = [.. subject.Teachers
                .Where(s => !s.IsDeleted)
            ];

            foreach (ListViewItem item in subjectTeachersListView.Items) {
                if (selectedTeachers.Any(s => s.Id == (int)item.Tag!)) {
                    item.Checked = true;
                }
            }

            subjectConfirmButton.Text = "Confirm (edit)";
            _bufferEditSubject = subject;
        }
        void EditModule(Module module) {
            EditSubject(module.Subject);
            ListViewItem? item = subjectModulesListView.Items
                .Cast<ListViewItem>()
                .FirstOrDefault(i => i.SubItems[0].Text == module.Number.ToString());

            subjectModulesListView.BackColor = Color.Gainsboro;

            if (item == null) {
                // TODO ERRO;
                mainTabControl.SelectedTab = spectatorTabPage;
                return;
            }

            _bufferEditModule = _bufferAddedModules.FirstOrDefault(m =>
                m.Id == module.Id
            );
            if (_bufferEditModule == null) {
                // TODO ERRO;
                mainTabControl.SelectedTab = spectatorTabPage;
                return;
            }

            moduleNumberNumericUpDown.Value = module.Number;
            moduleNameTextBox.Text = module.Name;

            string[] parts = IntToTimeString(module.DurationMin) // "00h 00m"
            .Split(' ');                                         // ["00h", "00m"]
            string strHours = parts[0].Replace("h", "");
            string strMinutes = parts[1].Replace("m", "");

            moduleDurationMaskedTextBox.Text = $"{strHours,3}h {strMinutes,2}m";

            addModuleButton.Text = "Confirm";

            MessageBox.Show(
                "Editing a module requires to edit the subject too",
                "EDITING MODULE",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }

        ListViewItem? item = spectatorListView.GetItemAt(_bufferPoint.Value.x, _bufferPoint.Value.y);
        _bufferPoint = null;

        if (item == null) {
            return;
        }
        if (spectatorComboBox.SelectedItem == null) {
            return;
        }

        string option = spectatorComboBox.SelectedItem.ToString()!;
        bool isUser = option == "User"
                   || option == "Student"
                   || option == "Teacher"
                   || option == "Secretary";
        bool isSituation = option == "Situation";
        bool isClass = option == "Class";
        bool isCourse = option == "Course";
        bool isSubject = option == "Subject";
        bool isModule = option == "Module";

        if (isUser) {
            int userId = (int)item.Tag!;
            User? user = dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.Subjects)
                .First(u => u.Id == userId);
            if (user == null) {
                return;
            }
            EditUser(user);

        } else if (isSituation) {
            int situationId = (int)item.Tag!;
            Situation? situation = dbContext.Situations
                .Include(s => s.Requests)
                .FirstOrDefault(s => s.Id == situationId);
            if (situation == null) {
                return;
            }
            EditSituation(situation);
        } else if (isClass) {
            int classId = (int)item.Tag!;
            Class? @class = dbContext.Classes
                .Include(c => c.Users)
                .Include(c => c.Course)
                .FirstOrDefault(c => c.Id == classId);
            if (@class == null) {
                return;
            }
            EditClass(@class);
        } else if (isCourse) {
            int courseId = (int)item.Tag!;
            Course? course = dbContext.Courses
                .Include(c => c.Subjects)
                .FirstOrDefault(c => c.Id == courseId);
            if (course == null) {
                return;
            }
            EditCourse(course);
        } else if (isSubject) {
            int subjectId = (int)item.Tag!;
            Subject? subject = dbContext.Subjects
                .Include(s => s.Teachers)
                .Include(s => s.Modules)
                .FirstOrDefault(s => s.Id == subjectId);
            if (subject == null) {
                return;
            }
            EditSubject(subject);
        } else if (isModule) {
            int moduleId = (int)item.Tag!;
            Module? module = dbContext.Modules
                .Include(m => m.Subject)
                .FirstOrDefault(m => m.Id == moduleId);
            if (module == null) {
                return;
            }
            EditModule(module);
        }
    }

    private void ClearTabPage(TabPage? tabPage) {
        if (tabPage == null) {
            return;
        }

        int tabPageIndex = mainTabControl.TabPages.IndexOf(tabPage);

        if (tabPageIndex == -1) {
            return;
        }

        tabPage.BackColor = Color.WhiteSmoke;

        AppTabPage appTabPage = (AppTabPage)tabPageIndex;

        switch (appTabPage) {
        case AppTabPage.UserCreation:
            _bufferEditUser = null;
            identificationTextBox.Text = "";
            nifMaskedTextBox.Text = "";
            roleComboBox.SelectedItem = null;
            firstNameTextBox.Text = "";
            lastNameTextBox.Text = "";
            emailTextBox.Text = "";
            passwordTextBox.Text = "";
            identificationTextBox.Enabled = true;
            photoPictureBox.Image = null;
            Helper.ListViewCheckedItemsClear(userSubjectsListView);
            userConfirmButton.Text = "Confirm";
            birthDateDateTimePicker.Value = DateTime.Parse("1 / 1 / 2000");
            break;

        case AppTabPage.SituationCreation:
            _bufferEditSituation = null;
            situationEnableDateCheckBox.Checked = false;
            situationNameTextBox.Text = "";
            situationTaxNumericUpDown.Value = 0;
            situationStartDateMaskedTextBox.Text = "";
            situationEndDateMaskedTextBox.Text = "";
            situationConfirmButton.Text = "Confirm";
            break;

        case AppTabPage.ClassCreation:
            _bufferAddedStudents.Clear();
            _bufferEditClass = null;
            classNameTextBox.Text = "";
            classMaxStudentsNumericUpDown.Value = 1;
            classCourseComboBox.SelectedItem = null;
            classSubjectsReadOnlyListView.Items.Clear();
            classShowAllStudentsCheckBox.Checked = false;
            classCourseComboBox.Enabled = true;
            showAllStudentsCheckBox_CheckedChanged(null!, null!);
            Helper.ListViewCheckedItemsClear(classStudentsListView);
            classConfirmButton.Text = "Confirm";
            break;

        case AppTabPage.CourseCreation:
            _bufferEditCourse = null;
            _bufferAddedSubjects.Clear();
            courseNameTextBox.Text = "";
            courseAbbreviationTextBox.Text = "";
            Helper.ListViewCheckedItemsClear(courseSubjectsListView);
            courseConfirmButton.Text = "Confirm";
            break;

        case AppTabPage.SubjectCreation:
            _bufferAddedModules.Clear();
            subjectModulesListView.BackColor = Color.White;
            _bufferEditSubject = null;
            _bufferEditModule = null;
            subjectModulesListView.Items.Clear();
            subjectAbbreviationTextBox.Text = "";
            subjectNameTextBox.Text = "";
            moduleNumberNumericUpDown.Value = 1;
            moduleNameTextBox.Text = "";
            moduleDurationMaskedTextBox.Text = "";
            Helper.ListViewCheckedItemsClear(subjectTeachersListView);
            subjectConfirmButton.Text = "Confirm";
            addModuleButton.Text = "Add";
            addModuleButton.Enabled = true;
            break;
        }
    }

    private void classComboBox_KeyDown(object sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Back) {
            classComboBox.SelectedItem = null;
        }
    }

    private void cancelEditToolStripMenuItem_Click(object sender, EventArgs e) {
        if (_bufferEditModule != null) {
            subjectModulesListView.BackColor = Color.White;
            addModuleButton.Text = "Add";
            moduleDurationMaskedTextBox.Text = "";
            moduleNameTextBox.Text = "";
            bool isRepeatedModuleNumber(int n) => subjectModulesListView.Items
            .Cast<ListViewItem>()
            .Any(i => i.SubItems[0].Text == n.ToString());

            moduleNumberNumericUpDown.Value = 1;
            while (isRepeatedModuleNumber((int)moduleNumberNumericUpDown.Value)) {
                moduleNumberNumericUpDown.Value++;
            }
            _bufferEditModule = null;
        }

        var bufferTabMapping = new (object? buffer, TabPage tabPage)[] {
            (_bufferEditUser     , userCreationTabPage),
            (_bufferEditSituation, situationCreationTabPage),
            (_bufferEditClass    , classCreationTabPage),
            (_bufferEditCourse   , courseCreationTabPage),
            (_bufferEditSubject  , subjectCreationTabPage),
        };
        ClearTabPage(
            bufferTabMapping.FirstOrDefault(t => t.buffer != null).tabPage
        );
    }

    private void userConfirmButton_MouseDown(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right && _bufferEditUser != null) {
            cancelEditContextMenuStrip.Show(userConfirmButton, e.Location);
        }
    }

    private void situationConfirmButton_MouseDown(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right && _bufferEditSituation != null) {
            cancelEditContextMenuStrip.Show(situationConfirmButton, e.Location);
        }
    }

    private void classConfirmButton_MouseDown(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right && _bufferEditClass != null) {
            cancelEditContextMenuStrip.Show(classConfirmButton, e.Location);
        }
    }

    private void courseConfirmButton_MouseDown(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right && _bufferEditCourse != null) {
            cancelEditContextMenuStrip.Show(courseConfirmButton, e.Location);
        }
    }

    private void addModuleButton_MouseDown(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right && _bufferEditModule != null) {
            cancelEditContextMenuStrip.Show(addModuleButton, e.Location);
        }
    }

    private void subjectConfirmButton_MouseDown(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right && _bufferEditSituation != null) {
            cancelEditContextMenuStrip.Show(subjectConfirmButton, e.Location);
        }
    }

    private void spectatorDeleteToolStripMenuItem_Click(object sender, EventArgs e) {
        if (_bufferPoint == null) {
            return;
        }

        using var dbContext = new EamDbContext();

        void DeleteUser(User user) {
            user.IsDeleted = true;

            // cancela os requests independente se é aluno ou professor
            foreach (Request request in user.RequestStudents.Concat(user.RequestTeachers)) {
                bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                       || request.StatusId == NotApprovedStatusId
                                       || request.StatusId == CanceledStatusId;
                if (!isRequestCompleted) {
                    request.StatusId = CanceledStatusId;
                    var requestHistory = new RequestHistory {
                        Datetime = DateTime.Now,
                        RequestId = request.Id,
                        StatusId = CanceledStatusId,
                        UserId = user.Id,
                    };
                    dbContext.RequestHistories.Add(requestHistory);
                    dbContext.Update(request);
                }
            }

            dbContext.Update(user);
            dbContext.SaveChanges();
        }
        void DeleteSituation(Situation situation) {
            situation.IsDeleted = true;
            dbContext.Update(situation);
            dbContext.SaveChanges();
        }
        void DeleteClass(Class @class) {
            @class.IsDeleted = true;
            foreach (User student in @class.Users) {
                student.Class = null;
                student.ClassId = null; // só para garantir

                // como o aluno fica sem turma, vamos cancelar os pedidos dele
                foreach (Request request in student.RequestStudents) {
                    bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                           || request.StatusId == NotApprovedStatusId
                                           || request.StatusId == CanceledStatusId;
                    if (!isRequestCompleted) {
                        request.StatusId = CanceledStatusId;
                        var requestHistory = new RequestHistory {
                            Datetime = DateTime.Now,
                            RequestId = request.Id,
                            StatusId = CanceledStatusId,
                            UserId = student.Id,
                        };
                        dbContext.RequestHistories.Add(requestHistory);
                        dbContext.Update(request);
                    }
                }
                dbContext.Update(student);
            }
            dbContext.Update(@class);
            dbContext.SaveChanges();
        }
        void DeleteCourse(Course course) {
            course.IsDeleted = true;
            foreach (Class @class in course.Classes) {
                DeleteClass(@class);
            }
            dbContext.Update(course);
            dbContext.SaveChanges();
        }
        void DeleteSubject(Subject subject) {
            subject.IsDeleted = true;
            foreach (Module module in subject.Modules) {
                DeleteModule(module);
            }
            dbContext.Update(subject);
            dbContext.SaveChanges();
        }
        void DeleteModule(Module module) {
            module.IsDeleted = true;
            foreach (Request request in module.Requests) {
                bool isRequestCompleted = request.StatusId == ReleasedStatusId
                                       || request.StatusId == NotApprovedStatusId
                                       || request.StatusId == CanceledStatusId;
                if (!isRequestCompleted) {
                    request.StatusId = CanceledStatusId;
                    var requestHistory = new RequestHistory {
                        Datetime = DateTime.Now,
                        RequestId = request.Id,
                        StatusId = CanceledStatusId,
                        UserId = request.TeacherId,
                    };
                    dbContext.RequestHistories.Add(requestHistory);
                    dbContext.Update(request);
                }
            }
            dbContext.Update(module);
            dbContext.SaveChanges();
        }

        ListViewItem? item = spectatorListView.GetItemAt(_bufferPoint.Value.x, _bufferPoint.Value.y);
        _bufferPoint = null;

        if (item == null) {
            return;
        }
        if (spectatorComboBox.SelectedItem == null) {
            return;
        }

        string option = spectatorComboBox.SelectedItem.ToString()!;
        bool isUser = option == "User"
                   || option == "Student"
                   || option == "Teacher"
                   || option == "Secretary";
        bool isSituation = option == "Situation";
        bool isClass = option == "Class";
        bool isCourse = option == "Course";
        bool isSubject = option == "Subject";
        bool isModule = option == "Module";

        if (isUser) {
            int userId = (int)item.Tag!;
            User? user = dbContext.Users
                .Include(u => u.Role)
                .Include(u => u.RequestStudents)
                .Include(u => u.RequestTeachers)
                .Include(u => u.RequestHistories)
                .First(u => u.Id == userId);
            if (user == null || user.IsDeleted) {
                return;
            }

            DialogResult result = MessageBox.Show(
                "Deleting this user will cancel all their pending requests!\n" +
                "Are you sure you want to proceed?",
                "ARE YOU SURE?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes) {
                DeleteUser(user);
            }
        } else if (isSituation) {
            int situationId = (int)item.Tag!;
            Situation? situation = dbContext.Situations
                .Include(s => s.Requests)
                .FirstOrDefault(s => s.Id == situationId);
            if (situation == null || situation.IsDeleted) {
                return;
            }
            DialogResult result = MessageBox.Show(
                "Are you sure you want to proceed?",
                "ARE YOU SURE?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (result == DialogResult.Yes) {
                DeleteSituation(situation);
            }
        } else if (isClass) {
            int classId = (int)item.Tag!;
            Class? @class = dbContext.Classes
                .Include(c => c.Users)
                    .ThenInclude(s => s.RequestStudents)
                .Include(c => c.Course)
                .FirstOrDefault(c => c.Id == classId);
            if (@class == null || @class.IsDeleted) {
                return;
            }
            DialogResult result = MessageBox.Show(
                "Deleting this class will remove all students from it!\n" +
                "Removing a class from a student will cancel their pending requests!\n" +
                "Are you sure you want to proceed?",
                "ARE YOU SURE?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (result == DialogResult.Yes) {
                DeleteClass(@class);
            }
        } else if (isCourse) {
            int courseId = (int)item.Tag!;
            Course? course = dbContext.Courses
                .Include(c => c.Classes)
                    .ThenInclude(c => c.Users)
                        .ThenInclude(s => s.RequestStudents)
                .Include(c => c.Subjects)
                    .ThenInclude(s => s.Modules)
                        .ThenInclude(m => m.Requests)
                .FirstOrDefault(c => c.Id == courseId);
            if (course == null) {
                return;
            }
            DialogResult result = MessageBox.Show(
                "Deleting this course will delete all associated classes!\n" +
                "Removing a class from a student will cancel their pending requests!\n" +
                "Are you sure you want to proceed?",
                "ARE YOU SURE?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (result == DialogResult.Yes) {
                DeleteCourse(course);
            }
        } else if (isSubject) {
            int subjectId = (int)item.Tag!;
            Subject? subject = dbContext.Subjects
                .Include(s => s.Teachers)
                .Include(s => s.Modules)
                    .ThenInclude(m => m.Requests)
                .FirstOrDefault(s => s.Id == subjectId);
            if (subject == null) {
                return;
            }
            DialogResult result = MessageBox.Show(
                "Deleting this subject will delete all associated modules!\n" +
                "Deleting a module will cancel all associated pending requests!\n" +
                "Are you sure you want to proceed?",
                "ARE YOU SURE?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (result == DialogResult.Yes) {
                DeleteSubject(subject);
            }
        } else if (isModule) {
            int moduleId = (int)item.Tag!;
            Module? module = dbContext.Modules
                .Include(m => m.Subject)
                .Include(m => m.Requests)
                .FirstOrDefault(m => m.Id == moduleId);
            if (module == null) {
                return;
            }
            DialogResult result = MessageBox.Show(
                "Deleting this module will will cancel all associated pending requests!\n" +
                "Are you sure you want to proceed?",
                "ARE YOU SURE?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );
            if (result == DialogResult.Yes) {
                DeleteModule(module);
            }
        }
        spectatorComboBox.DroppedDown = true;
        spectatorComboBox.DroppedDown = false;
    }

    private void classMaxStudentsNumericUpDown_ValueChanged(object sender, EventArgs e) {
        if (classMaxStudentsNumericUpDown.Value < _bufferAddedStudents.Count) {
            classMaxStudentsNumericUpDown.Value++;
        }
    }

    private void spectatorRefreshToolStripMenuItem_Click(object sender, EventArgs e) {
        spectatorComboBox_SelectedIndexChanged(sender, e);
    }

    private void spectatorCaseSensitiveCheckBox_CheckedChanged(object sender, EventArgs e) {
        spectatorComboBox_SelectedIndexChanged(sender, e);
    }

    private void spectatorListView_DoubleClick(object sender, EventArgs e) {
        if (spectatorComboBox.SelectedItem?.ToString() != "Request") {
            return;
        }
        using var dbContext = new EamDbContext();

        int? requestId = (int?)spectatorListView.SelectedItems[0]?.Tag;
        if (requestId == null) {
            // TODO mensagem
            return;
        }
        var request = dbContext.Requests
            .Include(r => r.RequestHistories)
             .FirstOrDefault(r => r.Id == requestId);

        if (request == null) {
            // TODO mensagem
            return;
        }

        var requestHistoryForm = new RequestHistoryForm(request) {
            Text = request.Number
        };
        requestHistoryForm.Show();
    }

    private void spectatorRequestsToolStripMenuItem_Click(object sender, EventArgs e) {
        _bufferUserRequests = null;
        _bufferStudentRequests = null;
        _bufferTeacherRequests = null;
        _bufferSecretaryRequests = null;
        _bufferSubjectRequests = null;
        _bufferModuleRequests = null;
        _bufferCourseRequests = null;

        if (spectatorComboBox.SelectedItem == null) {
            return;
        }

        if (_bufferPoint == null) {
            return;
        }

        string option = spectatorComboBox.SelectedItem.ToString()!;

        ListViewItem? item = spectatorListView.GetItemAt(_bufferPoint.Value.x, _bufferPoint.Value.y);
        _bufferPoint = null;

        if (item == null) {
            return;
        }

        using var dbContext = new EamDbContext();

        switch (option) {
        case "User":
            int userId = (int)item.Tag!;
            var user = dbContext.Users.FirstOrDefault(u => u.Id == userId);
            _bufferUserRequests = user;
            break;

        case "Student":
            int studentId = (int)item.Tag!;
            var student = dbContext.Users.FirstOrDefault(s => s.Id == studentId);
            _bufferStudentRequests = student;
            break;

        case "Teacher":
            int teacherId = (int)item.Tag!;
            var teacher = dbContext.Users.FirstOrDefault(t => t.Id == teacherId);
            _bufferTeacherRequests = teacher;
            break;

        case "Secretary":
            int secretaryId = (int)item.Tag!;
            var secretary = dbContext.Users.FirstOrDefault(s => s.Id == secretaryId);
            _bufferSecretaryRequests = secretary;
            break;

        case "Subject":
            int subjectId = (int)item.Tag!;
            var subject = dbContext.Subjects.FirstOrDefault(s => s.Id == subjectId);
            _bufferSubjectRequests = subject;
            break;

        case "Module":
            int moduleId = (int)item.Tag!;
            var module = dbContext.Modules.FirstOrDefault(m => m.Id == moduleId);
            _bufferModuleRequests = module;
            break;

        case "Course":
            int courseId = (int)item.Tag!;
            var course = dbContext.Courses.FirstOrDefault(m => m.Id == courseId);
            _bufferCourseRequests = course;
            break;
        }
        spectatorComboBox.SelectedItem = "Request";
    }

    private void spectatorRemoveFilterToolStripMenuItem_Click(object sender, EventArgs e) {
        _bufferUserRequests = null;
        _bufferStudentRequests = null;
        _bufferTeacherRequests = null;
        _bufferSecretaryRequests = null;
        _bufferSubjectRequests = null;
        _bufferModuleRequests = null;
        _bufferCourseRequests = null;
        spectatorComboBox_SelectedIndexChanged(sender, e);
    }

    private void userSubjectsListView_MouseDown(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            ListViewItem? item = userSubjectsListView.GetItemAt(e.X, e.Y);
            subjectsRefreshToolStripMenuItem.Visible = item == null;

            _bufferPoint = (e.X, e.Y);
            subjectsContextMenuStrip.Show(userSubjectsListView, e.Location);
        }
    }

    private void subjecstRefreshToolStripMenuItem_Click(object sender, EventArgs e) {
        using var dbContext = new EamDbContext();

        IEnumerable<Subject> subjects = dbContext.Subjects
                .Where(s => !s.IsDeleted)
                .Include(s => s.Modules);

        if (_currentTabPage == AppTabPage.UserCreation) {

            IEnumerable<(ColumnHeader, Expression<Func<Subject, object>>)> subject_selectors = [
                ( new ColumnHeader { Text = "Abbreviation", Width = 90, TextAlign = HorizontalAlignment.Left }
                    , s => s.Abbreviation ),
                    ( new ColumnHeader { Text = "Name", Width = 300, TextAlign = HorizontalAlignment.Left }
                    , s => s.Name ),
                    ( new ColumnHeader { Text = "Modules", Width = 70, TextAlign = HorizontalAlignment.Right }
                    , s => s.Modules.Count ),
                    ( new ColumnHeader { Text = "Duration", Width = 90, TextAlign = HorizontalAlignment.Right }
                    , s => IntToTimeString(s.Modules.Sum(m => m.DurationMin)) ),
                ];

            userSubjectsListView.ItemChecked -= userSubjectsListView_ItemChecked;
            Helper.InjectToListView(userSubjectsListView, subjects, subject_selectors);
            userSubjectsListView.ItemChecked += userSubjectsListView_ItemChecked;

            for (int i = 0; i < subjects.Count(); i++) {
                userSubjectsListView.Items[i].Tag = subjects.ElementAt(i).Id;
            }

            for (int i = 0; i < subjects.Count(); i++) {
                int id = (int)userSubjectsListView.Items[i].Tag!;
                if (_bufferAddedSubjects.Any(s => s.Id == id)) {
                    userSubjectsListView.ItemChecked -= userSubjectsListView_ItemChecked;
                    userSubjectsListView.Items[i].Checked = true;
                    userSubjectsListView.ItemChecked += userSubjectsListView_ItemChecked;
                }
            }
        } else {
            IEnumerable<(ColumnHeader, Expression<Func<Subject, object>>)> subject_selectors = [
                ( new ColumnHeader { Text = "Abbreviation", Width = 90, TextAlign = HorizontalAlignment.Left }
                , s => s.Abbreviation ),
                ( new ColumnHeader { Text = "Name", Width = 550, TextAlign = HorizontalAlignment.Left }
                , s => s.Name ),
                ( new ColumnHeader { Text = "Modules", Width = 70, TextAlign = HorizontalAlignment.Right }
                , s => s.Modules.Count ),
                ( new ColumnHeader { Text = "Duration", Width = 90, TextAlign = HorizontalAlignment.Right }
                , s => IntToTimeString(s.Modules.Sum(m => m.DurationMin)) ),
            ];

            courseSubjectsListView.ItemChecked -= courseSubjectsListView_ItemChecked;
            Helper.InjectToListView(courseSubjectsListView, subjects, subject_selectors);
            courseSubjectsListView.ItemChecked += courseSubjectsListView_ItemChecked;

            for (int i = 0; i < subjects.Count(); i++) {
                courseSubjectsListView.ItemChecked -= courseSubjectsListView_ItemChecked;
                courseSubjectsListView.Items[i].Tag = subjects.ElementAt(i).Id;
                courseSubjectsListView.ItemChecked += courseSubjectsListView_ItemChecked;
            }
        }
        _bufferPoint = null;
    }

    private void courseSubjectsListView_MouseDown(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            ListViewItem? item = courseSubjectsListView.GetItemAt(e.X, e.Y);
            subjectsRefreshToolStripMenuItem.Visible = item == null;

            _bufferPoint = (e.X, e.Y);
            subjectsContextMenuStrip.Show(courseSubjectsListView, e.Location);
        }
    }
}   