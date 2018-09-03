using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraScheduler;

namespace GanttChartDeneme
{
    public partial class Form1 : Form
    {
        private int LastSplitContainerControlSplitterPosition;

        public Form1()
        {
            InitializeComponent();

            #region #AppointmentEvents
            schedulerStorage1.AppointmentsInserted += new PersistentObjectsEventHandler(schedulerStorage1_AppointmentsInserted);
            schedulerStorage1.AppointmentsChanged += new PersistentObjectsEventHandler(schedulerStorage1_AppointmentsChanged);
            schedulerStorage1.AppointmentsDeleted += new PersistentObjectsEventHandler(schedulerStorage1_AppointmentsDeleted);
            #endregion #AppointmentEvents
            #region #AppointmentDependencyEvents
            schedulerStorage1.AppointmentDependenciesInserted += new PersistentObjectsEventHandler(schedulerStorage1_AppointmentDependenciesInserted);
            schedulerStorage1.AppointmentDependenciesChanged += new PersistentObjectsEventHandler(schedulerStorage1_AppointmentDependenciesChanged);
            schedulerStorage1.AppointmentDependenciesDeleted += new PersistentObjectsEventHandler(schedulerStorage1_AppointmentDependenciesDeleted);
            #endregion #AppointmentDependencyEvents

            //Fix the view type and splitter position.
            schedulerControl1.ActiveViewChanged += new EventHandler(schedulerControl1_ActiveViewChanged);
            this.splitContainerControl1.SplitterPositionChanged += new System.EventHandler(this.splitContainerControl1_SplitterPositionChanged);

            #region #changescales
            TimeScaleCollection scales = schedulerControl1.TimelineView.Scales;

            scales.BeginUpdate();
            try
            {
                scales.Clear();
                scales.Add(new CustomTimeScaleDay());
                scales.Add(new CustomTimeScaleHour());
            }
            finally
            {
                scales.EndUpdate();
            }
            #endregion #changescales
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LastSplitContainerControlSplitterPosition = 180;

            // TODO: This line of code loads data into the 'gantTestDataSet1.TaskDependencies' table. You can move, or remove it, as needed.
            this.taskDependenciesTableAdapter.Fill(this.gantTestDataSet1.TaskDependencies);
            // TODO: This line of code loads data into the 'gantTestDataSet1.Resources' table. You can move, or remove it, as needed.
            this.resourcesTableAdapter.Fill(this.gantTestDataSet1.Resources);
            // TODO: This line of code loads data into the 'gantTestDataSet1.Appointments' table. You can move, or remove it, as needed.
            this.appointmentsTableAdapter.Fill(this.gantTestDataSet1.Appointments);

            #region #CommitIdToDataSource
            schedulerStorage1.Appointments.CommitIdToDataSource = false;
            this.appointmentsTableAdapter.Adapter.RowUpdated += new SqlRowUpdatedEventHandler(appointmentsTableAdapter_RowUpdated);
            #endregion #CommitIdToDataSource

            #region #Adjustment
            schedulerControl1.ActiveViewType = SchedulerViewType.Gantt;
            schedulerControl1.GroupType = SchedulerGroupType.Resource;
            schedulerControl1.GanttView.CellsAutoHeightOptions.Enabled = true;
            // Hide unnecessary visual elements.
            schedulerControl1.GanttView.ShowResourceHeaders = false;
            //schedulerControl1.GanttView.NavigationButtonVisibility = NavigationButtonVisibility.Never;
            // Disable user sorting in the Resource Tree (clicking the column will not change the sort order).
            colDescription.OptionsColumn.AllowSort = false;
            #endregion #Adjustment

            schedulerControl1.GanttView.Scales.Clear();
            schedulerControl1.GanttView.Scales.Add(new CustomTimeScaleDay());
            schedulerControl1.GanttView.Scales.Add(new CustomTimeScaleHour());
            schedulerControl1.GanttView.Scales.Add(new CustomTimeScale15Minutes());

            schedulerControl1.Start = DateTime.Now.Date.AddHours(+8);

            schedulerControl1.Resize += schedulerControl1_Resize;
            SetCellWidth();
        }

        void SetCellWidth()
        {
            schedulerControl1.GanttView.Scales[1].Width = schedulerControl1.Width / 11;
            schedulerControl1.GanttView.Scales[2].Width = schedulerControl1.Width / 44;
        }

        private void schedulerStorage1_AppointmentsChanged(object sender, PersistentObjectsEventArgs e)
        {
            CommitTask();
        }

        private void schedulerStorage1_AppointmentsDeleted(object sender, PersistentObjectsEventArgs e)
        {
            CommitTask();
        }

        private void schedulerStorage1_AppointmentsInserted(object sender, PersistentObjectsEventArgs e)
        {
            CommitTask();
            schedulerStorage1.SetAppointmentId(((Appointment)e.Objects[0]), id);
        }

        void CommitTask()
        {
            appointmentsTableAdapter.Update(gantTestDataSet1);
            this.gantTestDataSet1.AcceptChanges();
        }

        private void schedulerStorage1_AppointmentDependenciesChanged(object sender, PersistentObjectsEventArgs e)
        {
            CommitTaskDependency();
        }

        private void schedulerStorage1_AppointmentDependenciesDeleted(object sender, PersistentObjectsEventArgs e)
        {
            CommitTaskDependency();
        }

        private void schedulerStorage1_AppointmentDependenciesInserted(object sender, PersistentObjectsEventArgs e)
        {
            CommitTaskDependency();
        }

        void CommitTaskDependency()
        {
            taskDependenciesTableAdapter.Update(this.gantTestDataSet1);
            this.gantTestDataSet1.AcceptChanges();
        }

        int id = 0;

        private void appointmentsTableAdapter_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
        {
            if (e.Status == UpdateStatus.Continue && e.StatementType == StatementType.Insert)
            {
                id = 0;
                using (SqlCommand cmd = new SqlCommand("SELECT @@IDENTITY", appointmentsTableAdapter.Connection))
                {
                    id = Convert.ToInt32(cmd.ExecuteScalar());
                    e.Row["UniqueId"] = id;
                }
            }
        }

        private void schedulerControl1_ActiveViewChanged(object sender, EventArgs e)
        {
            this.splitContainerControl1.SplitterPositionChanged -= new System.EventHandler(this.splitContainerControl1_SplitterPositionChanged);
            bool isGanttView = schedulerControl1.ActiveViewType == SchedulerViewType.Gantt;
            try
            {
                splitContainerControl1.SplitterPosition = (isGanttView) ? LastSplitContainerControlSplitterPosition : 0;
            }
            finally
            {
                this.splitContainerControl1.SplitterPositionChanged += new System.EventHandler(this.splitContainerControl1_SplitterPositionChanged);
            }
        }

        private void splitContainerControl1_SplitterPositionChanged(object sender, EventArgs e)
        {
            LastSplitContainerControlSplitterPosition = splitContainerControl1.SplitterPosition;
        }

        private void schedulerControl1_Resize(object sender, EventArgs e)
        {
            SetCellWidth();
        }

        private void schedulerControl1_AppointmentDrop(object sender, AppointmentDragEventArgs e)
        {
            TimeSpan shift = e.EditedAppointment.Start - e.SourceAppointment.Start;
            Appointment parentAppointment = e.SourceAppointment;
            AppointmentBaseCollection dependentAppointments = GetDependentAppointments(parentAppointment);

            schedulerStorage1.BeginUpdate();
            foreach(Appointment dependentAppointment in dependentAppointments)
            {
                dependentAppointment.Start = dependentAppointment.Start.Add(shift);
            }
            schedulerStorage1.EndUpdate();
        }

        private AppointmentBaseCollection GetDependentAppointments(Appointment parentAppointment)
        {
            AppointmentBaseCollection dependentAppointments = new AppointmentBaseCollection();
            AppointmentDependencyBaseCollection collection = schedulerStorage1.AppointmentDependencies.Items.GetDependenciesByParentId(parentAppointment.Id);
            for (int i = 0; i < collection.Count; i++)
            {
                Object dependentId = collection[i].DependentId;
                Appointment dependentAppointment = schedulerStorage1.Appointments.GetAppointmentById(dependentId);
                dependentAppointments.Add(dependentAppointment);
            }

            return dependentAppointments;
        }
    }
}
