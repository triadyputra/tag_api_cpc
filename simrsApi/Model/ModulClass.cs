using cpcApi.Model.DTO;

namespace cpcApi.Model
{
    public class ModulClass
    {
        private List<MenuViewModel> _mvcMenu;
        private List<ControllerViewModel> _mvcConntroller;

        public List<MenuViewModel> MenuApp()
        {
            return new List<MenuViewModel>{
                new MenuViewModel{
                    IdMenu = "Beranda",
                    NoUrut = 1,
                    NamaMenu = "Beranda",
                },

                new MenuViewModel{
                    IdMenu = "Konfigurasi",
                    NoUrut = 2,
                    NamaMenu = "Konfigurasi",
                },

                new MenuViewModel{
                    IdMenu = "MasterData",
                    NoUrut = 3,
                    NamaMenu = "Master Data",
                },

                new MenuViewModel{
                    IdMenu = "Logistik",
                    NoUrut = 4,
                    NamaMenu = "Logistik",
                },

                new MenuViewModel{
                    IdMenu = "CPC",
                    NoUrut = 5,
                    NamaMenu = "Cash Processing Center",
                },
            };
        }

        public List<ControllerViewModel> Controller()
        {
            return new List<ControllerViewModel>{
                #region Beranda
                new ControllerViewModel{
                    IdController = "Beranda",
                    NoUrut = 1,
                    Controller = "Beranda",
                    IdMenu = "Beranda",
                },
                #endregion
              
                #region Konfigurasi
                new ControllerViewModel{
                    IdController = "Role",
                    NoUrut = 1,
                    Controller = "Role",
                    IdMenu = "Konfigurasi",
                },
                new ControllerViewModel{
                    IdController = "Akun",
                    NoUrut = 2,
                    Controller = "Akun",
                    IdMenu = "Konfigurasi",
                },
                new ControllerViewModel{
                    IdController = "AuditLogin",
                    NoUrut = 2,
                    Controller = "User Login",
                    IdMenu = "Konfigurasi",
                },
                #endregion

                #region master-data
                new ControllerViewModel{
                    IdController = "MasterCabang",
                    NoUrut = 1,
                    Controller = "Master Cabang",
                    IdMenu = "MasterData",
                },
                new ControllerViewModel{
                    IdController = "MasterMesin",
                    NoUrut = 2,
                    Controller = "Master Mesin",
                    IdMenu = "MasterData",
                },
                new ControllerViewModel{
                    IdController = "MasterKaset",
                    NoUrut = 3,
                    Controller = "Master Kaset",
                    IdMenu = "MasterData",
                },
                new ControllerViewModel{
                    IdController = "MasterBank",
                    NoUrut = 4,
                    Controller = "Master Bank",
                    IdMenu = "MasterData",
                },
                new ControllerViewModel{
                    IdController = "MasterMerekKaset",
                    NoUrut = 5,
                    Controller = "Master Merk Kaset",
                    IdMenu = "MasterData",
                },
                new ControllerViewModel{
                    IdController = "MasterDenom",
                    NoUrut = 6,
                    Controller = "Master Denom",
                    IdMenu = "MasterData",
                },
                #endregion
          
                #region Logistik
                new ControllerViewModel{
                    IdController = "RegisterSeal",
                    NoUrut = 1,
                    Controller = "Register Seal",
                    IdMenu = "Logistik",
                },
                #endregion
          
                #region cpc
                new ControllerViewModel{
                    IdController = "OrderCpc",
                    NoUrut = 1,
                    Controller = "Order Pengisian Kaset",
                    IdMenu = "CPC",
                },
                new ControllerViewModel{
                    IdController = "PengembalianKaset",
                    NoUrut = 1,
                    Controller = "Pengembalian Sisa Lokasi",
                    IdMenu = "CPC",
                },
                new ControllerViewModel{
                    IdController = "ProsesPersiapanUang",
                    NoUrut = 1,
                    Controller = "Proses Persiapan Uang",
                    IdMenu = "CPC",
                },
                new ControllerViewModel{
                    IdController = "TrackingKaset",
                    NoUrut = 1,
                    Controller = "Tracking Kaset",
                    IdMenu = "CPC",
                },
                // Vault
                new ControllerViewModel{
                    IdController = "StokVault",
                    NoUrut = 1,
                    Controller = "Stok Vault",
                    IdMenu = "CPC",
                },
                #endregion
            };
        }

        public List<ActionViewModel> Action()
        {
            return new List<ActionViewModel>{
                #region Beranda
                new ActionViewModel{
                    IdAction = "Read",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "Beranda",
                },
                new ActionViewModel{
                    IdAction = "ProgresPemutahiran",
                    NoUrut = 2,
                    NamaAction = "Progres Pemutahiran",
                    IdController = "Beranda",
                },
                #endregion

                #region Konfigurasi
                /* Master Role */
                new ActionViewModel{
                    IdAction = "GetListRole",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "Role",
                },
                new ActionViewModel{
                    IdAction = "PostRole",
                    NoUrut = 2,
                    NamaAction = "Tambah",
                    IdController = "Role",
                },
                new ActionViewModel{
                    IdAction = "PutRole",
                    NoUrut = 3,
                    NamaAction = "Edit",
                    IdController = "Role",
                },
                new ActionViewModel{
                    IdAction = "DeleteRole",
                    NoUrut = 4,
                    NamaAction = "Hapus",
                    IdController = "Role",
                },

                /* Master Akun */
                new ActionViewModel{
                    IdAction = "GetListAkun",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "Akun",
                },
                new ActionViewModel{
                    IdAction = "PostAkun",
                    NoUrut = 2,
                    NamaAction = "Tambah",
                    IdController = "Akun",
                },
                new ActionViewModel{
                    IdAction = "PutAkun",
                    NoUrut = 3,
                    NamaAction = "Edit",
                    IdController = "Akun",
                },
                new ActionViewModel{
                    IdAction = "DeleteAkun",
                    NoUrut = 4,
                    NamaAction = "Hapus",
                    IdController = "Akun",
                },

                 /* Audit Login */
                new ActionViewModel{
                    IdAction = "GetListAuditLogin",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "AuditLogin",
                },

                #endregion

                #region master-data
                /* Master Cabang */
                new ActionViewModel{
                    IdAction = "GetListCabang",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "MasterCabang",
                },
                new ActionViewModel{
                    IdAction = "PostCabang",
                    NoUrut = 2,
                    NamaAction = "Tambah",
                    IdController = "MasterCabang",
                },
                new ActionViewModel{
                    IdAction = "PutCabang",
                    NoUrut = 3,
                    NamaAction = "Edit",
                    IdController = "MasterCabang",
                },
                new ActionViewModel{
                    IdAction = "DeleteCabang",
                    NoUrut = 4,
                    NamaAction = "Hapus",
                    IdController = "MasterCabang",
                },

                /* Master Mesin */
                new ActionViewModel{
                    IdAction = "GetListMesin",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "MasterMesin",
                },
                new ActionViewModel{
                    IdAction = "PostMesin",
                    NoUrut = 2,
                    NamaAction = "Tambah",
                    IdController = "MasterMesin",
                },
                new ActionViewModel{
                    IdAction = "PutMesin",
                    NoUrut = 3,
                    NamaAction = "Edit",
                    IdController = "MasterMesin",
                },
                new ActionViewModel{
                    IdAction = "DeleteMesin",
                    NoUrut = 4,
                    NamaAction = "Hapus",
                    IdController = "MasterMesin",
                },


                /* Master Kaset */
                new ActionViewModel{
                    IdAction = "GetListMasterKaset",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "MasterKaset",
                },
                new ActionViewModel{
                    IdAction = "PostMasterKaset",
                    NoUrut = 2,
                    NamaAction = "Tambah",
                    IdController = "MasterKaset",
                },
                new ActionViewModel{
                    IdAction = "PutMasterKaset",
                    NoUrut = 3,
                    NamaAction = "Edit",
                    IdController = "MasterKaset",
                },
                new ActionViewModel{
                    IdAction = "DeleteMasterKaset",
                    NoUrut = 4,
                    NamaAction = "Hapus",
                    IdController = "MasterKaset",
                },
                new ActionViewModel{
                    IdAction = "DownloadTemplate",
                    NoUrut = 5,
                    NamaAction = "Download Templete",
                    IdController = "MasterKaset",
                },
                new ActionViewModel{
                    IdAction = "UploadKaset",
                    NoUrut = 6,
                    NamaAction = "Upload Data",
                    IdController = "MasterKaset",
                },

                 /* Master Bank */
                new ActionViewModel{
                    IdAction = "GetListMasterBank",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "MasterBank",
                },
                new ActionViewModel{
                    IdAction = "PostMasterBank",
                    NoUrut = 2,
                    NamaAction = "Tambah",
                    IdController = "MasterBank",
                },
                new ActionViewModel{
                    IdAction = "PutMasterBank",
                    NoUrut = 3,
                    NamaAction = "Edit",
                    IdController = "MasterBank",
                },
                new ActionViewModel{
                    IdAction = "DeleteMasterBank",
                    NoUrut = 4,
                    NamaAction = "Hapus",
                    IdController = "MasterBank",
                },

                /* Master Merk Kaset */
                new ActionViewModel{
                    IdAction = "GetListMasterMerekKaset",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "MasterMerekKaset",
                },
                new ActionViewModel{
                    IdAction = "PostMasterMerekKaset",
                    NoUrut = 2,
                    NamaAction = "Tambah",
                    IdController = "MasterMerekKaset",
                },
                new ActionViewModel{
                    IdAction = "PutMasterMerekKaset",
                    NoUrut = 3,
                    NamaAction = "Edit",
                    IdController = "MasterMerekKaset",
                },
                new ActionViewModel{
                    IdAction = "DisableMasterMerekKaset",
                    NoUrut = 4,
                    NamaAction = "Hapus",
                    IdController = "MasterMerekKaset",
                },

                /* Master Denom */
                new ActionViewModel{
                    IdAction = "GetListMasterDenom",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "MasterDenom",
                },
                new ActionViewModel{
                    IdAction = "PostMasterDenom",
                    NoUrut = 2,
                    NamaAction = "Tambah",
                    IdController = "MasterDenom",
                },
                new ActionViewModel{
                    IdAction = "PutMasterDenom",
                    NoUrut = 3,
                    NamaAction = "Edit",
                    IdController = "MasterDenom",
                },
                new ActionViewModel{
                    IdAction = "DeleteMasterDenom",
                    NoUrut = 4,
                    NamaAction = "Hapus",
                    IdController = "MasterDenom",
                },

                #endregion

                #region Logistik
                /* Register Seal */
                new ActionViewModel{
                    IdAction = "GetListRegisterSeal",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "RegisterSeal",
                },
                new ActionViewModel{
                    IdAction = "PostRegisterSeal",
                    NoUrut = 2,
                    NamaAction = "Tambah",
                    IdController = "RegisterSeal",
                },
                new ActionViewModel{
                    IdAction = "PutRegisterSeal",
                    NoUrut = 3,
                    NamaAction = "Edit",
                    IdController = "RegisterSeal",
                },
                #endregion

                #region cpc
                /* Order Pengisian kaset */
                new ActionViewModel{
                    IdAction = "GetListOrderCpc",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "OrderCpc",
                },
                new ActionViewModel{
                    IdAction = "PostOrderCpc",
                    NoUrut = 2,
                    NamaAction = "Proses",
                    IdController = "OrderCpc",
                },
                new ActionViewModel{
                    IdAction = "FinalOrderCpc",
                    NoUrut = 3,
                    NamaAction = "Final",
                    IdController = "OrderCpc",
                },

                 /* Pengembalian sisa lokasi */
                new ActionViewModel{
                    IdAction = "GetListPengembalianKaset",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "PengembalianKaset",
                },
                new ActionViewModel{
                    IdAction = "PostPengembalianKaset",
                    NoUrut = 2,
                    NamaAction = "Proses",
                    IdController = "PengembalianKaset",
                },
                new ActionViewModel{
                    IdAction = "PutPengembalianKaset",
                    NoUrut = 3,
                    NamaAction = "Edit",
                    IdController = "PengembalianKaset",
                },
                new ActionViewModel{
                    IdAction = "DeletePengembalianKaset",
                    NoUrut = 4,
                    NamaAction = "Hapus",
                    IdController = "PengembalianKaset",
                },


                /* Proses Persiapan uang */
                new ActionViewModel{
                    IdAction = "GetListProsesCpc",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "ProsesPersiapanUang",
                },
                new ActionViewModel{
                    IdAction = "SimpanDraftProsesCpc",
                    NoUrut = 2,
                    NamaAction = "Simpan Draft",
                    IdController = "ProsesPersiapanUang",
                },
                new ActionViewModel{
                    IdAction = "FinalisasiProsesCpc",
                    NoUrut = 3,
                    NamaAction = "Finalisasi",
                    IdController = "ProsesPersiapanUang",
                },
                new ActionViewModel{
                    IdAction = "DeleteDraftProsesCpc",
                    NoUrut = 4,
                    NamaAction = "Delete Draft",
                    IdController = "ProsesPersiapanUang",
                },

                /* Tracking Kaset */
                new ActionViewModel{
                    IdAction = "GetListTrackingKaset",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "TrackingKaset",
                },
                new ActionViewModel{
                    IdAction = "GetHistory",
                    NoUrut = 2,
                    NamaAction = "Detail Tracking",
                    IdController = "TrackingKaset",
                },


                /* Stock Vault */
                new ActionViewModel{
                    IdAction = "GetListStokVault",
                    NoUrut = 1,
                    NamaAction = "Lihat",
                    IdController = "StokVault",
                },
                new ActionViewModel{
                    IdAction = "GetMutasi",
                    NoUrut = 2,
                    NamaAction = "Detail Mutasi",
                    IdController = "StokVault",
                },
                new ActionViewModel{
                    IdAction = "PostMutasi",
                    NoUrut = 3,
                    NamaAction = "Mutasi",
                    IdController = "StokVault",
                },
                new ActionViewModel{
                    IdAction = "PostTransfer",
                    NoUrut = 4,
                    NamaAction = "Mutasi Antar Cabang",
                    IdController = "StokVault",
                },
                new ActionViewModel{
                    IdAction = "PostOpname",
                    NoUrut = 5,
                    NamaAction = "Opaname",
                    IdController = "StokVault",
                },
                
                #endregion
            };
        }

        public IEnumerable<MenuViewModel> GetListMenu()
        {
            if (_mvcMenu != null)
                return _mvcMenu;

            _mvcMenu = new List<MenuViewModel>();
            _mvcConntroller = new List<ControllerViewModel>();

            var items = this.MenuApp();
            foreach (var _menu in items)
            {
                var currentModule = new MenuViewModel
                {
                    IdMenu = _menu.IdMenu,
                    NoUrut = _menu.NoUrut,
                    NamaMenu = _menu.NamaMenu,
                };

                var ctr = this.Controller().Where(c => c.IdMenu == _menu.IdMenu).OrderBy(x => x.NoUrut).ToList();
                var controller = new List<ControllerViewModel>();
                foreach (var _ctr in ctr)
                {
                    var act = this.Action().Where(c => c.IdController == _ctr.IdController).OrderBy(x => x.NoUrut).ToList();
                    var action = new List<ActionViewModel>();
                    foreach (var _act in act)
                    {
                        action.Add(new ActionViewModel
                        {
                            IdAction = _act.IdAction,
                            NoUrut = _act.NoUrut,
                            NamaAction = _act.NamaAction,
                            IdController = _act.IdController,
                        });
                    }

                    controller.Add(new ControllerViewModel
                    {
                        IdController = _ctr.IdController,
                        NoUrut = _ctr.NoUrut,
                        Controller = _ctr.Controller,
                        IdMenu = _ctr.IdMenu,
                        ActionViewModel = action,
                    });

                }

                if (controller.Any())
                {
                    currentModule.ControllerViewModel = controller;
                    _mvcMenu.Add(currentModule);
                }
            }
            return _mvcMenu;

        }


    }
}
