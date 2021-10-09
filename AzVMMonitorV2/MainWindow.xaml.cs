﻿/*
MainWindow.xaml.cs
06.10.2021 22:22:09
Alexey Sedoykin
*/

namespace AzVMMonitorV2
{
    using AzVMMonitorCostsPerVM;
    using AzVMMonitorCostsPerVMDisk;
    using AzVMMonitorCostsPerVMNetwork;
    using AzVMMonitorCostTotalData;
    using Microsoft.Azure.Management.Compute.Fluent;
    using Microsoft.Azure.Management.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Xml.Linq;

    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Defines the WM_SYSCOMMAND.
        /// </summary>
        public const Int32 WM_SYSCOMMAND = 0x112;

        /// <summary>
        /// Defines the MF_SEPARATOR.
        /// </summary>
        public const Int32 MF_SEPARATOR = 0x800;

        /// <summary>
        /// Defines the MF_BYPOSITION.
        /// </summary>
        public const Int32 MF_BYPOSITION = 0x400;

        /// <summary>
        /// Defines the MF_STRING.
        /// </summary>
        public const Int32 MF_STRING = 0x0;

        /// <summary>
        /// Defines the _SettingsSysMenuID.
        /// </summary>
        public const Int32 _SettingsSysMenuID = 1000;

        /// <summary>
        /// Defines the _AboutSysMenuID.
        /// </summary>
        public const Int32 _AboutSysMenuID = 1001;

        /// <summary>
        /// The GetSystemMenu.
        /// </summary>
        /// <param name="windowH">The windowH<see cref="IntPtr"/>.</param>
        /// <param name="revert">The revert<see cref="bool"/>.</param>
        /// <returns>The <see cref="IntPtr"/>.</returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr windowH, bool revert);

        /// <summary>
        /// The InsertMenu.
        /// </summary>
        /// <param name="menuH">The menuH<see cref="IntPtr"/>.</param>
        /// <param name="positionW">The positionW<see cref="Int32"/>.</param>
        /// <param name="flagsW">The flagsW<see cref="Int32"/>.</param>
        /// <param name="idnewitem">The idnewitem<see cref="Int32"/>.</param>
        /// <param name="newitem">The newitem<see cref="string"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        [DllImport("user32.dll")]
        private static extern bool InsertMenu(IntPtr menuH, Int32 positionW, Int32 flagsW, Int32 idnewitem, string newitem);

        /// <summary>
        /// Gets the Handle.
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                return new WindowInteropHelper(this).Handle;
            }
        }

        /// <summary>
        /// Defines the AzureCred.
        /// </summary>
        internal IAzure AzureCred;

        /// <summary>
        /// Defines the AzureSubscriptionID.
        /// </summary>
        internal string AzureSubscriptionID = "";

        /// <summary>
        /// Defines the AzureTokenRESTAPI.
        /// </summary>
        internal string AzureTokenRESTAPI;

        /// <summary>
        /// Defines the ClientIdAppID.
        /// </summary>
        internal string ClientIdAppID = "";

        /// <summary>
        /// Defines the ClientSecretAppSecret.
        /// </summary>
        internal string ClientSecretAppSecret = "";

        /// <summary>
        /// Defines the ClientTenantId.
        /// </summary>
        internal string ClientTenantId = "";

        /// <summary>
        /// Defines the CostVM, CostDisks, CostNET.......
        /// </summary>
        internal double CostVM, CostDisks, CostNET;

        /// <summary>
        /// Defines the IsOkay.
        /// </summary>
        internal bool IsOkay = false;

        /// <summary>
        /// Defines the ItemsVM.
        /// </summary>
        internal List<VMHelper> ItemsVM = new List<VMHelper>();

        /// <summary>
        /// Defines the SelectedVM.
        /// </summary>
        internal VMHelper SelectedVM;

        /// <summary>
        /// Defines the TypeCurrency.
        /// </summary>
        internal string TypeCurrency = "";

        /// <summary>
        /// Defines the TypeTimeFrame.
        /// </summary>
        internal string TypeTimeFrame = "";

        /// <summary>
        /// Defines the VMPublcIP.
        /// </summary>
        internal string VMPublcIP = null;

        /// <summary>
        /// Defines the notifyIcon.
        /// </summary>
        private System.Windows.Forms.NotifyIcon notifyIcon = null;

        /// <summary>
        /// Defines the vmworkingTime.
        /// </summary>
        private string vmworkingTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public void ReadXMLConfig()
        {
            try
            {
                XDocument xmlConfigFile = XDocument.Load("configuration.xml");
                ClientIdAppID = xmlConfigFile.Descendants("clientID_appID").First().Value;
                ClientSecretAppSecret = xmlConfigFile.Descendants("clietnSecret_appSecret").First().Value;
                ClientTenantId = xmlConfigFile.Descendants("clientTenantId").First().Value;
                AzureSubscriptionID = xmlConfigFile.Descendants("azureSubscriptionID").First().Value;
                TypeCurrency = xmlConfigFile.Descendants("currency").First().Value;
                TypeTimeFrame = xmlConfigFile.Descendants("typeTimeFrame").First().Value;
            }
            catch (FileNotFoundException)
            {
                LabelWarningError.Visibility = Visibility.Visible;
                LabelWarningError.Text = "The something wrong: Configuration.xml is missing";
            }
        }

        /// <summary>
        /// The WndProc.
        /// </summary>
        /// <param name="hwnd">The hwnd<see cref="IntPtr"/>.</param>
        /// <param name="msg">The msg<see cref="int"/>.</param>
        /// <param name="paramW">The paramW<see cref="IntPtr"/>.</param>
        /// <param name="paramL">The paramL<see cref="IntPtr"/>.</param>
        /// <param name="handled">The handled<see cref="bool"/>.</param>
        /// <returns>The <see cref="IntPtr"/>.</returns>
        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr paramW, IntPtr paramL, ref bool handled)
        {
            if (msg == WM_SYSCOMMAND)
            {
                switch (paramW.ToInt32())
                {
                    case _SettingsSysMenuID:
                        ConfigurationWindow confWindow = new ConfigurationWindow();
                        confWindow.Show();
                        handled = true;
                        break;

                    case _AboutSysMenuID:
                        AboutWindow aboutWindow = new AboutWindow();
                        aboutWindow.Show();
                        handled = true;
                        break;
                }
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            //причесываем внешний элементов управления при запуске приложения
            ButtonStartVM.IsEnabled = false;
            ButtonStopVM.IsEnabled = false;
            ButtonRestartVM.IsEnabled = false;
            ButtonOpenRDP.IsEnabled = false;
            ButtonOpenAzurePortal.IsEnabled = false;
            ProgressDataLoadPanel.Visibility = Visibility.Hidden;
            TabFinanceData.IsEnabled = false;
            LabelWarningError.Visibility = Visibility.Hidden;

            //считываем паарметры из configuration.xml
            ReadXMLConfig();

            //задаем название для закладки с финансовыми показателями
            TabFinanceData.Header = "Fincance Data (in " + TypeCurrency + " for " + TypeTimeFrame + " period)";

            //делаем иконку в system tray и декларируем реацию на двойной щелчок по ней
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = new System.Drawing.Icon("icon_app.ico");
            notifyIcon.MouseDoubleClick +=
                new System.Windows.Forms.MouseEventHandler
                    (NotifyIconMouse_DoubleClick);
        }

        /// <summary>
        /// The Window_Loaded.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //НАЧАЛО блока расширение контекстного меню

            IntPtr systemMenuHandle = GetSystemMenu(this.Handle, false);
            InsertMenu(systemMenuHandle, 5, MF_BYPOSITION | MF_SEPARATOR, 0, string.Empty);
            InsertMenu(systemMenuHandle, 6, MF_BYPOSITION, _SettingsSysMenuID, "Settings");
            InsertMenu(systemMenuHandle, 7, MF_BYPOSITION, _AboutSysMenuID, "About");
            HwndSource source = HwndSource.FromHwnd(this.Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            //КОНЕЦ блока расширение контекстного меню

            //Начинаем грузить данные из Azure
            //блокируем некоторые кнопки и выводим панель с индикатором загрузки
            ButtonReloadData.IsEnabled = false;
            ProgressDataLoadPanel.Visibility = Visibility.Visible;

            //Грузим данные вывзывая функцию RefreshVMData в отдельном потоке
            var taskRefreshData = Task.Run(() =>
            {
                RefreshVMData();
            });

            taskRefreshData.ContinueWith((t) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (IsOkay == true)
                    {
                        //наполянем список имеющимися Virtual Machine-ами
                        ListOfVM.ItemsSource = ItemsVM;
                        //скрываем панель индикатора загрузки
                        ProgressDataLoadPanel.Visibility = Visibility.Hidden;
                        //выводим дату обновления данных из Azure и включаем кнопку запроса данных
                        LabelLastUpdate.Text = "Last update: " + DateTime.Now.ToString();
                        ButtonReloadData.IsEnabled = true;
                    }
                    else
                    {
                        //реакция на случае, если, что-то пошло не так походу обращения к Azure
                        ConnectionToAzureHasTrouble();
                    }
                });
            });
        }

        /// <summary>
        /// The MainWindowUI_StateChanged.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="EventArgs"/>.</param>
        private void MainWindowUI_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;

                notifyIcon.Text = "AzVM Monitor";

                notifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                notifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }

        /// <summary>
        /// The NotifyIconMouse_DoubleClick.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.Windows.Forms.MouseEventArgs"/>.</param>
        private void NotifyIconMouse_DoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        /// <summary>
        /// The UnlockLockUI.
        /// </summary>
        /// <param name="isLock">The isLock<see cref="bool"/>.</param>
        private void UnlockLockUI(bool isLock)
        {
            ButtonStartVM.IsEnabled = isLock;
            ButtonRestartVM.IsEnabled = isLock;
            ButtonStopVM.IsEnabled = isLock;
            ButtonOpenRDP.IsEnabled = isLock;
            ButtonReloadData.IsEnabled = isLock;
            ButtonOpenAzurePortal.IsEnabled = isLock;
            TabTechicalData.IsEnabled = isLock;

            ListOfVM.IsEnabled = isLock;
            if (isLock == false)
            {
                ProgressDataLoadPanel.Visibility = Visibility.Visible;
                COSTDetailsHyperLink.Visibility = Visibility.Hidden;
            }
            else if (isLock == true)
            {
                ProgressDataLoadPanel.Visibility = Visibility.Hidden;
                COSTDetailsHyperLink.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// The ConnectionToAzureHasTrouble.
        /// </summary>
        public void ConnectionToAzureHasTrouble()
        {
            LabelVMName.Background = Brushes.Red;
            LabelVMSize.Background = Brushes.Orange;
            LabelVMState.Background = Brushes.Yellow;
            LabelVMPublicIP.Background = Brushes.Green;

            LabelVMName.Text = "No conenction to AZURE, Enter credential data!";
            LabelVMSize.Text = "No conenction to AZURE, Enter credential data!";
            LabelVMWasCreated.Text = "No conenction to AZURE, Enter credential data!";
            LabelVMState.Text = "No conenction to AZURE, Enter credential data!";
            LabelVMPublicIP.Text = "No conenction to AZURE, Enter credential data!";

            ConfigurationWindow confWindow = new ConfigurationWindow();
            confWindow.ShowDialog();

            LabelVMName.Background = Brushes.LightGreen;
            LabelVMSize.Background = Brushes.LightGreen;
            LabelVMState.Background = Brushes.LightGreen;
            LabelVMPublicIP.Background = Brushes.LightGreen;

            LabelVMName.Text = "Please restart the application";
            LabelVMSize.Text = "Please restart the application";
            LabelVMState.Text = "Please restart the application";
            LabelVMPublicIP.Text = "Please restart the application";
        }

        /// <summary>
        /// The LogInAzure.
        /// </summary>
        /// <param name="clientId">The clientId<see cref="string"/>.</param>
        /// <param name="clientSecret">The clientSecret<see cref="string"/>.</param>
        /// <param name="clientTenantId">The clientTenantId<see cref="string"/>.</param>
        /// <returns>The <see cref="IAzure"/>.</returns>
        private IAzure LogInAzure(string clientId, string clientSecret, string clientTenantId)
        {
            var creds = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                clientId,
                clientSecret,
                clientTenantId,
                environment: AzureEnvironment.AzureGlobalCloud);

            return Azure.Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(creds)
                .WithDefaultSubscription();
        }

        /// <summary>
        /// The RefreshVMData.
        /// </summary>
        public void RefreshVMData()
        {
            ItemsVM.Clear();
            try
            {
                AzureTokenRESTAPI = AzBillingRESTHelper.
                   GetAuthorizationToken(ClientIdAppID, ClientSecretAppSecret, ClientTenantId);

                AzureCred = LogInAzure(ClientIdAppID, ClientSecretAppSecret, ClientTenantId);

                foreach (var IVirtualMachine in AzureCred.VirtualMachines.List().ToList())
                {
                    ItemsVM.Add(new VMHelper(IVirtualMachine));
                    /*
                    if (IVirtualMachine.PowerState.Value.Substring(11).ToString()=="running")
                    {
                        Console.WriteLine("This is a line.\nThis is another line.");
                        TimeSpan ValueOfWorkingTime = ((TimeSpan)(DateTime.Now - IVirtualMachine.InstanceView.Statuses[0].Time));

                        RunningVMNow += " "+IVirtualMachine.Name + " "+ SomeHelpers.TruncateString((TimeSpan.FromHours(ValueOfWorkingTime.Hours).TotalDays).ToString(), 4)+" days\n";
                    }
                    */
                }
                IsOkay = true;
            }
            catch
            {
                IsOkay = false;
            }
        }

        /// <summary>
        /// The GetFinancialData.
        /// </summary>
        private async void GetFinancialData()
        {
            LabelCOSTTotal.Text = "Getting data...";
            LabelCOSTPerVM.Text = "Getting data...";
            LabelCOSTPerVMDisk.Text = "Getting data...";
            LabelCOSTPerVMNetwork.Text = "Getting data...";
            LabelCOSTVMTotal.Text = "Calculating...";
            UnlockLockUI(false);
            //
            double totalVmDiskCosts = 0;
            //
            var task1GetTotalCOST = AzBillingRESTHelper.GetTotalCost(AzureTokenRESTAPI, AzureSubscriptionID);
            var task2GetCOSTPerVM = AzBillingRESTHelper.GetCostByVM(AzureTokenRESTAPI, AzureSubscriptionID, SelectedVM.VMName, SelectedVM.VMGroupName, TypeTimeFrame);
            var task3GetCOSTPerVMDisk = AzBillingRESTHelper.GetCostByVMDisk(AzureTokenRESTAPI, AzureSubscriptionID, SelectedVM.VMDiskFirst, SelectedVM.VMGroupName, TypeTimeFrame);
            var task4GetCOSTPerVMNetwork = AzBillingRESTHelper.GetCostByVMNetwork(AzureTokenRESTAPI, AzureSubscriptionID, SelectedVM.VMNetwork, SelectedVM.VMGroupName, TypeTimeFrame);

            await task1GetTotalCOST;
            var costDataTotal = CostDataTotal.FromJson(AzBillingRESTHelper.AzureDetails.ResponseCostDataTotal);

            await task2GetCOSTPerVM;
            var costDataPerVm = CostDataPerVm.FromJson(AzBillingRESTHelper.AzureDetails.ResponseCostDataPerVM);

            await task3GetCOSTPerVMDisk;
            var costDataPermVMDisk = CostDataPerVmDisk.FromJson(AzBillingRESTHelper.AzureDetails.ResponseCostDataPerVMDisk);

            await task4GetCOSTPerVMNetwork;
            var costDataPermVMNetwork = CostDataPerVmNetwork.FromJson(AzBillingRESTHelper.AzureDetails.ResponseCostDataPerVMNetwork);

            //выводим суммарную стоимость VM, если данных нет то exception
            try
            {
                LabelCOSTPerVM.Text = costDataPerVm.Properties.Rows[3][0].Double.Value.ToString();
                CostVM = costDataPerVm.Properties.Rows[3][0].Double.Value;
            }
            catch (System.IndexOutOfRangeException)
            {
                LabelCOSTPerVM.Text = "Data was not get";
                CostVM = 0;
            }

            if (TypeCurrency == "RUB")
            {
                //определяем сколько компонентов входит в диск и суммируем траты

                for (int i = 0; i < costDataPermVMDisk.Properties.Rows.Length; i++)
                {
                    totalVmDiskCosts += costDataPermVMDisk.Properties.Rows[i][0].Double.Value;
                }
            }
            else if (TypeCurrency == "USD")
            {
                //определяем сколько компонентов входит в диск и суммируем траты
                for (int i = 0; i < costDataPermVMDisk.Properties.Rows.Length; i++)
                {
                    totalVmDiskCosts += costDataPermVMDisk.Properties.Rows[i][1].Double.Value;
                }
            }

            //выводим суммарную стоимость по всем дискам у VM, если данных нет то exception
            try
            {
                LabelCOSTPerVMDisk.Text = totalVmDiskCosts.ToString();
                CostDisks = totalVmDiskCosts;
            }
            catch (System.IndexOutOfRangeException)
            {
                LabelCOSTPerVMDisk.Text = "Data was not get";
                CostDisks = 0;
            }

            //выводим суммарную стоимость по сетевой подсистеме VM, если данных нет то exception
            try
            {
                LabelCOSTPerVMNetwork.Text = costDataPermVMNetwork.Properties.Rows[0][0].Double.Value.ToString();
                CostNET = costDataPermVMNetwork.Properties.Rows[0][0].Double.Value;
            }
            catch (System.IndexOutOfRangeException)
            {
                LabelCOSTPerVMNetwork.Text = "Data was not get";
                CostNET = 0;
            }

            try
            {
                LabelCOSTTotal.Text
                 = costDataTotal.Properties.Rows[0][0].Double.Value.ToString().Remove(7);
            }
            catch (System.IndexOutOfRangeException)
            {
                LabelCOSTTotal.Text = "Data was not get";
            }

            LabelCOSTVMTotal.Text = (CostVM + CostDisks + CostNET).ToString();
            //
            LabelLastUpdate.Text = "Last update: " + DateTime.Now.ToString();
            //
            UnlockLockUI(true);
        }

        /// <summary>
        /// The ButtonOpenConfiguration_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void ButtonOpenConfiguration_Click(object sender, RoutedEventArgs e)
        {
            ConfigurationWindow confWindow = new ConfigurationWindow();
            confWindow.Show();
        }

        /// <summary>
        /// The ButtonOpenRDP_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void ButtonOpenRDP_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("mstsc", "/v:" + SelectedVM.VMPublicIP);
        }

        /// <summary>
        /// The ButtonRestartVM_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void ButtonRestartVM_Click(object sender, RoutedEventArgs e)
        {
            MainWindowUI.Title = "AzVMMonitor - Restarting " + SelectedVM.VMName + " please wait...";
            UnlockLockUI(false);

            var taskRestartVM = Task.Run(() =>
            {
                IVirtualMachine vm = SelectedVM.VMCurrent;
                vm.Restart();
                RefreshVMData();
            });

            taskRestartVM.ContinueWith((tt) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (IsOkay == true)
                    {
                        ListOfVM.ItemsSource = ItemsVM;
                        ListOfVM.Items.Refresh();
                        MainWindowUI.Title = "AzVMMonitor - The " + SelectedVM.VMName + " was restarted";
                        LabelLastUpdate.Text = "Last update: " + DateTime.Now.ToString();
                        UnlockLockUI(true);
                    }
                    else
                    {
                        ConnectionToAzureHasTrouble();
                    }
                });
            });
        }

        /// <summary>
        /// The ButtonStartVM_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void ButtonStartVM_Click(object sender, RoutedEventArgs e)
        {
            MainWindowUI.Title = "AzVMMonitor - Starting " + SelectedVM.VMName + " please wait...";
            UnlockLockUI(false);

            var taskStartVM = Task.Run(() =>
            {
                IVirtualMachine vm = SelectedVM.VMCurrent;
                vm.Start();
                RefreshVMData();
            });

            taskStartVM.ContinueWith((tt) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (IsOkay == true)
                    {
                        ListOfVM.ItemsSource = ItemsVM;
                        ListOfVM.Items.Refresh();
                        MainWindowUI.Title = "AzVMMonitor - The " + SelectedVM.VMName + " was started";
                        LabelLastUpdate.Text = "Last update: " + DateTime.Now.ToString();
                        UnlockLockUI(true);
                    }
                    else
                    {
                        ConnectionToAzureHasTrouble();
                    }
                });
            });
        }

        /// <summary>
        /// The ButtonStopVM_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void ButtonStopVM_Click(object sender, RoutedEventArgs e)
        {
            MainWindowUI.Title = "AzVMMonitor - Stopping " + SelectedVM.VMName + " please wait...";
            UnlockLockUI(false);

            var taskStopVM = Task.Run(() =>
            {
                IVirtualMachine vm = SelectedVM.VMCurrent;

                vm.Deallocate();
                RefreshVMData();
            });

            taskStopVM.ContinueWith((tt) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (IsOkay == true)
                    {
                        ListOfVM.ItemsSource = ItemsVM;
                        ListOfVM.Items.Refresh();
                        MainWindowUI.Title = "AzVMMonitor - The " + SelectedVM.VMName + " was stopped";
                        LabelLastUpdate.Text = "Last update: " + DateTime.Now.ToString();
                        UnlockLockUI(true);
                    }
                    else
                    {
                        ConnectionToAzureHasTrouble();
                    }
                });
            });
        }

        /// <summary>
        /// The ButtonReloadData_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void ButtonReloadData_Click(object sender, RoutedEventArgs e)
        {
            UnlockLockUI(false);
            ReadXMLConfig();
            TabFinanceData.Header = "Fincance Data (in " + TypeCurrency + " for " + TypeTimeFrame + " period)";
            ProgressDataLoadPanel.Visibility = Visibility.Visible;
            var taskRefreshData = Task.Run(() =>
            {
                RefreshVMData();
            });

            taskRefreshData.ContinueWith((t) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (IsOkay == true)
                    {
                        ListOfVM.ItemsSource = ItemsVM;
                        ProgressDataLoadPanel.Visibility = Visibility.Hidden;
                        LabelLastUpdate.Text = "Last update: " + DateTime.Now.ToString();
                        UnlockLockUI(true);
                    }
                    else
                    {
                        ConnectionToAzureHasTrouble();
                    }
                });
            });
        }

        /// <summary>
        /// The ListOfVM_PreviewMouseLeftButtonDown.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="MouseButtonEventArgs"/>.</param>
        private void ListOfVM_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// The ListOfVM_PreviewMouseLeftButtonUp.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="MouseButtonEventArgs"/>.</param>
        private void ListOfVM_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UnlockLockUI(true);
            try
            {
                vmworkingTime = null;
                LabelVMStateToolTip.Content = "";
                LabelVMState.Background = Brushes.White;

                SelectedVM = (VMHelper)this.ListOfVM.SelectedItem;
                if (SelectedVM != null)
                {
                    //определяем состояние выбранной VM, работает или нет
                    if (SelectedVM.VMState == "running")
                    {
                        //Управляем доступностью кнопок старт/стоп/перезапуск для выбранной VM в состоянии работает
                        ButtonStartVM.IsEnabled = false;
                        ButtonStopVM.IsEnabled = true;
                        ButtonRestartVM.IsEnabled = true;
                        ButtonOpenRDP.IsEnabled = true;

                        //Получаем время запуска VM и приводим его к локальном времени (из UTC)
                        DateTime utcVMStartTime = DateTime.Parse(SelectedVM.VMCurrent.InstanceView.Statuses[0].Time.ToString());
                        DateTime localVMStartTime = utcVMStartTime.ToLocalTime();
                        TimeSpan valueOfWorkinTime = ((TimeSpan)(DateTime.Now - localVMStartTime));

                        /* для отладки
                        Console.WriteLine(SelectedVM.VMCurrent.Name+" "+ SelectedVM.VMCurrent.InstanceView.Statuses[0].ToString()+"  "+SelectedVM.VMCurrent.InstanceView.Statuses[0].Time.ToString());
                        Console.WriteLine("Current Time: " + DateTime.Now.ToString());
                        Console.WriteLine("Started Time (UTC): " + SelectedVM.VMCurrent.InstanceView.Statuses[0].Time.ToString());
                        Console.WriteLine("Started Time (LOCAL): " + LocalVMStartTime.ToString());

                        Console.WriteLine("Working Time_raw: " + ValueOfWorkingTime.TotalHours.ToString());
                        Console.WriteLine("Time Was Spent (Hours): " + ValueOfWorkingTime.TotalHours.ToString());
                        Console.WriteLine("Time Was Spent (Hours-Days): " + (TimeSpan.FromHours(ValueOfWorkingTime.TotalHours)).TotalDays.ToString());
                         */

                        //В зависимости от вреемни работы меняем цветовой фон у поля LabelVMState
                        if ((TimeSpan.FromHours(valueOfWorkinTime.TotalHours)).TotalDays <= 0.2)
                        {
                            LabelVMState.Background = Brushes.Aquamarine;
                            vmworkingTime = "It Works around: " + SomeHelpers.TruncateString((TimeSpan.FromHours(valueOfWorkinTime.TotalHours).TotalDays).ToString(), 4) + " days";
                        }
                        else
                        if ((TimeSpan.FromHours(valueOfWorkinTime.TotalHours)).TotalDays <= 0.5)
                        {
                            LabelVMState.Background = Brushes.GreenYellow;
                            vmworkingTime = "It Works around: " + SomeHelpers.TruncateString((TimeSpan.FromHours(valueOfWorkinTime.TotalHours).TotalDays).ToString(), 4) + " days";
                        }
                        else
                        if ((TimeSpan.FromHours(valueOfWorkinTime.TotalHours)).TotalDays <= 1)
                        {
                            LabelVMState.Background = Brushes.DarkSeaGreen;
                            vmworkingTime = "It Works around: " + SomeHelpers.TruncateString((TimeSpan.FromHours(valueOfWorkinTime.TotalHours).TotalDays).ToString(), 4) + " days";
                        }
                        else if ((TimeSpan.FromHours(valueOfWorkinTime.TotalHours)).TotalDays <= 7)
                        {
                            LabelVMState.Background = Brushes.Gold;
                            vmworkingTime = "It Works around: " + SomeHelpers.TruncateString((TimeSpan.FromHours(valueOfWorkinTime.TotalHours).TotalDays).ToString(), 4) + " days";
                        }
                        else if ((TimeSpan.FromHours(valueOfWorkinTime.TotalHours)).TotalDays <= 14)
                        {
                            LabelVMState.Background = Brushes.Orange;
                            vmworkingTime = "It Works around: " + SomeHelpers.TruncateString((TimeSpan.FromHours(valueOfWorkinTime.TotalHours).TotalDays).ToString(), 4) + " days";
                        }
                        else if ((TimeSpan.FromHours(valueOfWorkinTime.TotalHours)).TotalDays <= 28)
                        {
                            LabelVMState.Background = Brushes.OrangeRed;
                            vmworkingTime = "It Works around: " + SomeHelpers.TruncateString((TimeSpan.FromHours(valueOfWorkinTime.TotalHours).TotalDays).ToString(), 4) + " days";
                        }
                    }
                    //Если выбранная VM не запущена - даем возможност её запустить
                    else
                    {
                        ButtonStartVM.IsEnabled = true;
                        ButtonStopVM.IsEnabled = false;
                        ButtonOpenRDP.IsEnabled = false;
                        //
                        vmworkingTime = null;
                        LabelVMStateToolTip.Content = "";
                        LabelVMState.Background = Brushes.White;
                    }
                }
                //Заполяем инфомрационные поля о выбранной VM
                TabFinanceData.IsEnabled = true;
                LabelVMName.Text = SelectedVM.VMName;
                LabelVMSize.Text = SelectedVM.VMSize;
                LabelVMState.Text = SelectedVM.VMState;
                LabelVMPublicIP.Text = SelectedVM.VMPublicIP;
                IDisk disk = AzureCred.Disks.GetById(SelectedVM.VMOsDiskID);
                LabelVMWasCreated.Text = disk.Inner.TimeCreated.Value.ToString();

                if (TabTechicalData.IsFocused)
                {
                    TabFinanceData.IsEnabled = true;
                    LabelVMName.Text = SelectedVM.VMName;
                    LabelVMSize.Text = SelectedVM.VMSize;
                    LabelVMState.Text = SelectedVM.VMState;
                    LabelVMPublicIP.Text = SelectedVM.VMPublicIP;
                }
                else if (TabFinanceData.IsSelected)
                {
                    GetFinancialData();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// The TabItemFinData_PreviewMouseLeftButtonUpAsync.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="MouseButtonEventArgs"/>.</param>
        private async void TabItemFinData_PreviewMouseLeftButtonUpAsync(object sender, MouseButtonEventArgs e)
        {
            GetFinancialData();
        }

        /// <summary>
        /// The TabItemTechData_PreviewMouseLeftButtonUp.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="MouseButtonEventArgs"/>.</param>
        private void TabItemTechData_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// The ButtonOpenAzurePortal_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void ButtonOpenAzurePortal_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://portal.azure.com/#@/resource/subscriptions/" + AzureSubscriptionID + "/resourceGroups/" + SelectedVM.VMGroupName + "/providers/Microsoft.Compute/virtualMachines/" + SelectedVM.VMCurrent.Name + "/overview");
        }

        /// <summary>
        /// The TextBlock_MouseDown.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="MouseButtonEventArgs"/>.</param>
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://portal.azure.com/#blade/Microsoft_Azure_CostManagement/Menu/costanalysis");
        }

        /// <summary>
        /// The LabelVMState_MouseEnter.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="MouseEventArgs"/>.</param>
        private void LabelVMState_MouseEnter(object sender, MouseEventArgs e)
        {
            if (vmworkingTime != null)
            {
                LabelVMStateToolTip.Content = vmworkingTime;
                LabelVMStateToolTip.Visibility = Visibility.Visible;
            }
            else
            {
                LabelVMStateToolTip.Visibility = Visibility.Hidden;
            }
        }
    }
}