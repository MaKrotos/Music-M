using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VK_UI3.DB;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static VK_UI3.DB.AccountsDB;
using static VK_UI3.Views.SectionView;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace VK_UI3.Views.LoginWindow
{
    public sealed partial class AccountList : UserControl
    {




     
        public static WeakEventManager onUpdateAccounts = new WeakEventManager();

        public AccountList()
        {
            this.InitializeComponent();
            this.Loading += AccountList_Loading;
            this.Loaded += AccountList_Loaded;
        }
        public ObservableCollection<Accounts> accounts { get; set; } = new ObservableCollection<Accounts>();

        private void AccountList_Loaded(object sender, RoutedEventArgs e)
        {

            updateAccounts();

        }

        private void AccountList_Loading(FrameworkElement sender, object args)
        {
            updateAccounts();
        }

        public void updateAccounts()
        {
            accounts.Clear();
            var accountss = AccountsDB.GetAllAccountsSorted();
            int i = 0;
            foreach (var item in accountss)
            {

                accounts.Add(item);
                if (item.id == AccountsDB.activeAccount.id)
                    AccountsListed.SelectedIndex = i;
                i++;
            }
            accounts.Add(new Accounts { });
        }
        private int previousSelectedAccount = -1;
        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.DispatcherQueue.TryEnqueue(async () =>
            {

                var selectedAccount = (Accounts)AccountsListed.SelectedItem;

                if (selectedAccount == null) return;
                if (selectedAccount.Token == null)
                {
                    if (previousSelectedAccount != -1)
                    {
                        AccountsListed.SelectedItem = previousSelectedAccount;
                    }
                    else
                    {
                        AccountsListed.SelectedIndex = -1;
                    }
                    AccountsDB.activeAccount = new AccountsDB.Accounts();
                    MainWindow.contentFrame.Navigate(typeof(Login), this, new DrillInNavigationTransitionInfo());
                    previousSelectedAccount = AccountsListed.SelectedIndex;
                }
                else
                {

                    if (accounts[AccountsListed.SelectedIndex].id == AccountsDB.activeAccount.id)
                        return;

                    AccountsDB.ActivateAccount(selectedAccount.id);
                    activeAccount = selectedAccount;


                }


            });
        }


        private async void AccountsList_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            var itemsToMove = new List<Accounts>(); // Create a list to store items for reordering
            var i = 0;

            foreach (var item in accounts)
            {
                if (item.Token == null && i != accounts.Count() - 1)
                {
                    // Mark items for reordering
                    itemsToMove.Add(item);

                }
            }

            foreach (var itemToMove in itemsToMove)
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    accounts.Move(accounts.IndexOf(itemToMove), accounts.Count() - 1);
                });
            }

            foreach (var item in accounts)
            {
                if (item.Token != null)
                {

                    item.sortID = i++;
                    item.Update();


                }
            }
        }

        private void AccountsList_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var items = (sender as ListView).Items;
            if (e.Items.Contains(items[items.Count - 1]))
            {
                e.Cancel = true;
            }
        }
    }
}