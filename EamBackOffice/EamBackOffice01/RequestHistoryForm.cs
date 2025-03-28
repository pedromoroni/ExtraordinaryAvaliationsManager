using EamBackOffice01.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EamBackOffice01;

public partial class RequestHistoryForm : Form {
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

    private readonly Request _request;
    public RequestHistoryForm(Request request) {
        InitializeComponent();
        _request = request;
    }

    private void RequestHistoryForm_Load(object sender, EventArgs e) {
        // titulo fica com o numero da request
        /* agr injetar pela seguinte ordem:
            * data/tempo -> quem foi -> modulo -> estado -> extra (caso o estado seja pago aparece o metodo, caso seja lançado aparece a nota)
            */
        using var dbContext = new EamDbContext();

        var requestHistory = dbContext.RequestHistories
            .Where(rh => rh.RequestId == _request.Id)
            .Include(rh => rh.Status)
            .Include(rh => rh.User)
                .ThenInclude(u => u.Role)
            .Include(rh => rh.Request)
                .ThenInclude(r => r.Module)
            .OrderDescending();

        IEnumerable<(ColumnHeader, Expression<Func<RequestHistory, object>>)> selectors = [
            ( new ColumnHeader { Text = "Date Time", Width = 120, TextAlign = HorizontalAlignment.Left }
            , rh => $"{rh.Datetime.ToShortDateString()} {rh.Datetime.ToShortTimeString()}" ),
            ( new ColumnHeader { Text = "User", Width = 150, TextAlign = HorizontalAlignment.Left }
            , rh => $"{rh.User.Identification} - {rh.User.FirstName} {rh.User.LastName}" ),
            ( new ColumnHeader { Text = "Role", Width = 90, TextAlign = HorizontalAlignment.Left }
            , rh => rh.User.Role.Name ),
            ( new ColumnHeader { Text = "Module", Width = 100, TextAlign = HorizontalAlignment.Left }
            , rh => $"{rh.Request.Module.Number} - {rh.Request.Module.Name}" ),
            ( new ColumnHeader { Text = "Status", Width = 130, TextAlign = HorizontalAlignment.Left }
            , rh => rh.Status.Description ),
            ( new ColumnHeader { Text = "Extra", Width = 100, TextAlign = HorizontalAlignment.Left }
            , rh => rh.StatusId == ReleasedStatusId ? $"{rh.Request.Grade!:00.00} / 20.00"
                  : rh.StatusId == PaidStatusId     ? rh.Request.PaymentMethod!
                  : "" ),
        ];

        Helper.InjectToListView(requestHistoryListView, requestHistory, selectors);
    }

    private void refreshToolStripMenuItem_Click(object sender, EventArgs e) {
        RequestHistoryForm_Load(sender, e);
    }

    private void requestHistoryListView_MouseDown(object sender, MouseEventArgs e) {
        if (e.Button == MouseButtons.Right) {
            refreshHistoryContextMenuStrip.Show(requestHistoryListView, e.Location);
        }
    }
}
