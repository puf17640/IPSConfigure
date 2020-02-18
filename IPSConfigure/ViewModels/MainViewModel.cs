using IndoorKonfiguration.Services;
using IPSConfigure.Models;
using IPSConfigure.Utilities;
using IPSConfigure.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using XF.Material.Forms.Models;
using XF.Material.Forms.UI.Dialogs;
using XF.Material.Forms.UI.Dialogs.Configurations;

namespace IPSConfigure.ViewModels
{
    public class MainViewModel : PropertyChangeAware
    {
        public string UserId => Settings.Current.UserId;


        public MainViewModel()
        {
            Models = DataStore.Instance.Rooms;
        }

        public string[] Actions => new string[] { "Edit", "Delete" };

        public string[] ListActions => new string[] { "Add Room", "Sort", "Clear All" };

        public ICommand ListMenuCommand => new Command<MaterialMenuResult>(async (s) => await ListMenuSelected(s));

        public ICommand MenuCommand => new Command<MaterialMenuResult>(async (s) => await MenuSelected(s));

        public ICommand RoomSelectedCommand => new Command<string>(async (s) => await ViewItemSelected(s));

        public ICommand UserIdCommand => new Command<string>(async (s) =>
        {
            await Clipboard.SetTextAsync(s);
            await MaterialDialog.Instance.SnackbarAsync(message: "UserId copied to clipboard.",
                                                msDuration: MaterialSnackbar.DurationShort, new MaterialSnackbarConfiguration()
                                                {
                                                    CornerRadius = 15f,
                                                    Margin = new Thickness(20, 0, 20, 20)
                                                });
        });

        private ObservableCollection<Room> _models;
        public ObservableCollection<Room> Models
        {
            get => _models;
            set => Set(ref _models, value);
        }

        private async Task ListMenuSelected(MaterialMenuResult s)
        {
            if (s.Index < 0 || s.Index > ListActions.Length)
                return;
            switch (ListActions[s.Index])
            {
                case "Add Room":
                    var input = await MaterialDialog.Instance.InputAsync("Add Room", "Enter the name of the room you want to add.", "", "Room", "Add", "Cancel", new MaterialInputDialogConfiguration()
                    {
                        ButtonAllCaps = true,
                        CornerRadius = 15f,
                    });
                    if (input.Length > 0)
                    {
                        var success = false;
                        try
                        {
                            success = await DataStore.Instance.AddRoomAsync(new Room()
                            {
                                Title = input,
                                User = Settings.Current.UserId
                            });
                        }
                        catch
                        {
                            // Log Error
                            success = false;
                        }
                        finally
                        {
                            await MaterialDialog.Instance.SnackbarAsync(message: success ? "Room saved." : "Oops! An error occured.",
                                                msDuration: MaterialSnackbar.DurationShort, new MaterialSnackbarConfiguration()
                                                {
                                                    CornerRadius = 15f,
                                                    Margin = new Thickness(20, 0, 20, 20)
                                                });
                        }
                    }
                    break;
                case "Sort":
                    this.Models = new ObservableCollection<Room>(DataStore.Instance.Rooms.OrderBy(m => m.Title));
                    await MaterialDialog.Instance.SnackbarAsync(message: "Rooms sorted.",
                                            msDuration: MaterialSnackbar.DurationShort, new MaterialSnackbarConfiguration()
                                            {
                                                CornerRadius = 15f,
                                                Margin = new Thickness(20, 0, 20, 20)
                                            });
                    break;
                case "Clear All":
                    if ((await MaterialDialog.Instance.ConfirmAsync("Are you sure you want to clear all your rooms?", "Your Rooms", "Yes", "No")).GetValueOrDefault())
                    {
                        var success = false;
                        try
                        {
                            success = Parallel.ForEach(this.Models, new ParallelOptions(), async (item, loop) =>
                            {
                                if (!await DataStore.Instance.DeleteRoomAsync(item))
                                    loop.Break();
                            }).IsCompleted;
                        }
                        catch
                        {
                            // Log Error
                            success = false;
                        }
                        finally
                        {
                            await MaterialDialog.Instance.SnackbarAsync(message: success ? "All rooms deleted." : "Oops! An error occured.",
                                                    msDuration: MaterialSnackbar.DurationShort, new MaterialSnackbarConfiguration()
                                                    {
                                                        CornerRadius = 15f,
                                                        Margin = new Thickness(20, 0, 20, 20)
                                                    });
                        }
                    }
                    break;
            }
        }

        private async Task ViewItemSelected(string id)
        {
            var selectedModel = Models.FirstOrDefault(m => m.Id == id);
            await Application.Current.MainPage.Navigation.PushAsync(new ConfigureRoomPage(selectedModel));
            App.CurrentRoom = selectedModel;
        }

        private async Task MenuSelected(MaterialMenuResult i)
        {
            var model = this.Models.FirstOrDefault(m => m.Id == (string)i.Parameter);
            if (model == null || i.Index < 0 || i.Index > Actions.Length)
                return;
            switch (Actions[i.Index])
            {
                case "Edit":
                    var input = await MaterialDialog.Instance.InputAsync("Edit Room", "Edit the selected room's name.", model.Title, "Room", "Confirm", "Cancel", new MaterialInputDialogConfiguration()
                    {
                        ButtonAllCaps = true,
                        CornerRadius = 15f,
                    });
                    if (input.Length > 0)
                    {
                        model.Title = input;
                        var success = false;
                        try
                        {
                            success = await DataStore.Instance.UpdateRoomAsync(model);
                        }
                        catch
                        {
                            // Log Error
                            success = false;
                        }
                        finally
                        {
                            await MaterialDialog.Instance.SnackbarAsync(message: success ? "Room updated." : "Oops! An error occured.",
                                                    msDuration: MaterialSnackbar.DurationShort, new MaterialSnackbarConfiguration()
                                                    {
                                                        CornerRadius = 15f,
                                                        Margin = new Thickness(20, 0, 20, 20)
                                                    });
                        }
                    }
                    break;
                case "Delete":
                    if ((await MaterialDialog.Instance.ConfirmAsync("Are you sure you want to delete this room?", model.Title, "Yes", "No")).GetValueOrDefault())
                    {
                        var success = false;
                        try
                        {
                            success = await DataStore.Instance.DeleteRoomAsync(model);
                        }
                        catch
                        {
                            // Log Error
                            success = false;
                        }
                        finally
                        {
                            await MaterialDialog.Instance.SnackbarAsync(message: success ? "Room deleted." : "Oops! An error occured.",
                                                    msDuration: MaterialSnackbar.DurationShort, new MaterialSnackbarConfiguration()
                                                    {
                                                        CornerRadius = 15f,
                                                        Margin = new Thickness(20, 0, 20, 20)
                                                    });
                        }
                    }
                    break;
            }
        }
    }
}
