﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Xml.Linq;
using System.IO;
using BE;


namespace DAL
{
    class DALXML : IDAL
    {
        #region Singleton
        private static readonly DALXML instance = new DALXML();

        //xelement
        private string hostingUnitPath;//where it's saved
        private XElement hostingUnits;

        public static DALXML Instance
        {
            get { return instance; }
        }
        #region c-tors
        private DALXML()
        {
            try
            {
                DownloadBank();//start bank download. in new thread?
            }
            catch
            {
                //throw? print error? retry?
            }

            //open xml files (creates if don't exit) and load items
            try
            {
                #region xelements load
                loadUnits();//puts units into xelement hostingUnits
                #endregion
            }
            catch(Exception ex)
            {
                throw new loadExceptionDAL(ex.Message);
            }

           
        }

        private void loadUnits()
        {
            try
            {
                hostingUnitPath = @"\hostingUnits.xml";//saves hostingUnit path
                if (File.Exists(hostingUnitPath))//file exists
                    hostingUnits = XElement.Load(hostingUnitPath);//loads units into hostingUnits
                else//creates it
                {
                    hostingUnits = new XElement("Unit Root");
                    hostingUnits.Save(hostingUnitPath);//saves
                }
            }
            catch
            {
                throw new loadExceptionDAL("Unable to load Hosting Units");
            }
        }

        static DALXML() { }
        #endregion
        #endregion

        #region banks
        //save banks
        public static volatile bool bankDownloaded = false;//flag if bank was downloaded
        void DownloadBank()
        {
            #region downloadBank
            const string xmlLocalPath = @"atm.xml";
            WebClient wc = new WebClient();
            try
            {
                string xmlServerPath =
               @"http://www.boi.org.il/he/BankingSupervision/BanksAndBranchLocations/Lists/BoiBankBranchesDocs/atm.xml";
                wc.DownloadFile(xmlServerPath, xmlLocalPath);
                bankDownloaded = true;
            }
            catch (Exception ex)
            {
                try
                {
                    string xmlServerPath = @"http://www.jct.ac.il/~coshri/atm.xml";
                    wc.DownloadFile(xmlServerPath, xmlLocalPath);
                    bankDownloaded = true;
                }
                catch (Exception exeption)
                {
                    //tries again if the connection didn't allow to download it
                }
            }
            finally
            {
                wc.Dispose();
            }
            #endregion

        }
        //List<BankAccount> GetBankAccounts()
        //{

        //}
        #endregion

        #region hostingUnits
        public List<HostingUnit> getAllHostingUnits()//xelement to hosting unit
                                                     //need to add convert diary

        {
            //converts xelement with units to list and returns it
            return (from host in hostingUnits.Elements()
             select new HostingUnit()//saves to new hosting unit
             {
                 HostingUnitKey = Convert.ToInt32(host.Element("Unit Key").Value),
                 HostingUnitName = Convert.ToString(host.Element("Unit Name").Value),
                 HostingUnitType = (Enums.HostingUnitType)(Enum.Parse(typeof(Enums.HostingUnitType), host.Element("Unit Type").Value)),
                 AreaVacation = (Enums.Area)(Enum.Parse(typeof(Enums.Area), host.Element("Unit Type").Value)),
                 NumAdult = Convert.ToInt32(host.Element("Adults").Value),
                 NumChildren = Convert.ToInt32(host.Element("Children").Value),
                 Pool = (Enums.Preference)(Enum.Parse(typeof(Enums.Preference), host.Element("Pool").Value)),
                 Garden = (Enums.Preference)(Enum.Parse(typeof(Enums.Preference), host.Element("Garden").Value)),
                 Jacuzzi = (Enums.Preference)(Enum.Parse(typeof(Enums.Preference), host.Element("Jacuzzi").Value)),
                 Meal = (Enums.MealType)(Enum.Parse(typeof(Enums.MealType), host.Element("Meal").Value)),
                 MoneyPaid=Convert.ToInt32(host.Element("Paid").Value),
                 //host
                 Host =new Host()
                 {
                     HostKey=Convert.ToInt32(host.Element("Host").Element("Host Key").Value),
                     Name=host.Element("Host").Element("Host Name").Value,
                     LastName=host.Element("Host").Element("Host Last").Value,
                     Mail=new System.Net.Mail.MailAddress(host.Element("Host").Element("Email").Value, host.Element("Host Name").Value + host.Element("Host Last").Value),
                     CollectionClearance=Convert.ToBoolean(host.Element("Host").Element("Clearance").Value),
                     Bank=new BankAccount()//bank
                     {
                         BankAcountNumber=Convert.ToInt32(host.Element("Host").Element("Bank").Element("Account Number").Value),
                         BankName=host.Element("Host").Element("Bank").Element("Bank Name").Value,
                         BankNumber=Convert.ToInt32(host.Element("Host").Element("Bank").Element("Bank Number").Value),
                         BranchNumber=Convert.ToInt32(host.Element("Host").Element("Bank").Element("Branch Number").Value),
                         BranchAddress= host.Element("Host").Element("Bank").Element("Branch Address").Value
                         //add
                     }
                 }
             }).ToList();
                //converts hostingunits to list
        }
        public HostingUnit findUnit(int unitKey)//returns unit with this unit key
        {
            try
            {
                return (from unit in hostingUnits.Elements()
                        where Convert.ToInt32(unit.Element("Unit Key").Value) == unitKey//this unit
                        select new HostingUnit()//saves to new hosting unit
                        {
                            HostingUnitKey = Convert.ToInt32(unit.Element("Unit Key").Value),
                            HostingUnitName = Convert.ToString(unit.Element("Unit Name").Value),
                            HostingUnitType = (Enums.HostingUnitType)(Enum.Parse(typeof(Enums.HostingUnitType), unit.Element("Unit Type").Value)),
                            AreaVacation = (Enums.Area)(Enum.Parse(typeof(Enums.Area), unit.Element("Unit Type").Value)),
                            NumAdult = Convert.ToInt32(unit.Element("Adults").Value),
                            NumChildren = Convert.ToInt32(unit.Element("Children").Value),
                            Pool = (Enums.Preference)(Enum.Parse(typeof(Enums.Preference), unit.Element("Pool").Value)),
                            Garden = (Enums.Preference)(Enum.Parse(typeof(Enums.Preference), unit.Element("Garden").Value)),
                            Jacuzzi = (Enums.Preference)(Enum.Parse(typeof(Enums.Preference), unit.Element("Jacuzzi").Value)),
                            Meal = (Enums.MealType)(Enum.Parse(typeof(Enums.MealType), unit.Element("Meal").Value)),
                            MoneyPaid = Convert.ToInt32(unit.Element("Paid").Value),
                            //host
                            Host = new Host()
                            {
                                HostKey = Convert.ToInt32(unit.Element("Host").Element("Key").Value),
                                Name = unit.Element("Host").Element("First Name").Value,
                                LastName = unit.Element("Host").Element("Last Name").Value,
                                Mail = new System.Net.Mail.MailAddress(unit.Element("Host").Element("Email").Value, unit.Element("Host Name").Value + unit.Element("Host Last").Value),
                                CollectionClearance = Convert.ToBoolean(unit.Element("Host").Element("Clearance").Value),
                                Bank = new BankAccount()//bank
                                {
                                    BankAcountNumber = Convert.ToInt32(unit.Element("Host").Element("Bank").Element("Account Number").Value),
                                    BankName = unit.Element("Host").Element("Bank").Element("Bank Name").Value,
                                    BankNumber = Convert.ToInt32(unit.Element("Host").Element("Bank").Element("Bank Number").Value),
                                    BranchNumber = Convert.ToInt32(unit.Element("Host").Element("Bank").Element("Branch Number").Value),
                                    BranchAddress = unit.Element("Host").Element("Bank").Element("Branch Address").Value
                                    //add branch city?
                                }
                            }
                        }).First();//returns first matching unit found
            }
            catch
            {
                return null;//unable to find unit 
            }
        }
        public void deleteUnit(HostingUnit toDelete)//deletes this unit
        {
            try
            {
                (from unit in hostingUnits.Elements()
                 where Convert.ToInt32(unit.Element("Unit Key").Value) == toDelete.HostingUnitKey//this unit
                 select unit).First().Remove();//removes first found
            }
            catch
            {
                throw new objectErrorDAL();//didn't find item
            }
            try
            {
                hostingUnits.Save(hostingUnitPath);
            }
            catch
            {
                throw new loadExceptionDAL("unable to save elements after deleting");//error in loading or saving the file
            }
           
            
        }

        public void changeUnit(HostingUnit hostingUnit1)//update unit
        {
            try
            {
                XElement host = (from unit in hostingUnits.Elements()
                                 where Convert.ToInt32(unit.Element("Unit Key").Value) == hostingUnit1.HostingUnitKey//this unit
                                 select unit).First();//first found
                //updates 

                host.Element("Unit Key").Value = hostingUnit1.HostingUnitKey.ToString();
                host.Element("Unit Name").Value = hostingUnit1.HostingUnitName;
                host.Element("Unit Type").Value = hostingUnit1.HostingUnitType.ToString();
                host.Element("Unit Area").Value = hostingUnit1.AreaVacation.ToString();
                host.Element("Adults").Value = hostingUnit1.NumAdult.ToString();
                host.Element("Children").Value = hostingUnit1.NumChildren.ToString();
                host.Element("Pool").Value = hostingUnit1.Pool.ToString();
                host.Element("Garden").Value = hostingUnit1.Garden.ToString();
                host.Element("Jacuzzi").Value = hostingUnit1.Jacuzzi.ToString();
                host.Element("Meal").Value = hostingUnit1.Meal.ToString();
                host.Element("Paid").Value = hostingUnit1.MoneyPaid.ToString();
                //host
                host.Element("Host").Element("Host Key").Value = hostingUnit1.Host.HostKey.ToString();
                host.Element("Host").Element("Host Name").Value = hostingUnit1.Host.Name;
                host.Element("Host").Element("Host Last").Value = hostingUnit1.Host.LastName;
                host.Element("Host").Element("Email").Value = hostingUnit1.Host.Mail.Address;
                host.Element("Host").Element("Clearance").Value = hostingUnit1.Host.CollectionClearance.ToString();
                host.Element("Host").Element("Bank").Element("Account Number").Value = hostingUnit1.Host.Bank.BankAcountNumber.ToString();
                host.Element("Host").Element("Bank").Element("Bank Name").Value = hostingUnit1.Host.Bank.BankName;
                host.Element("Host").Element("Bank").Element("Bank Number").Value = hostingUnit1.Host.Bank.BankNumber.ToString();
                host.Element("Host").Element("Bank").Element("Branch Number").Value = hostingUnit1.Host.Bank.BranchNumber.ToString();
                host.Element("Host").Element("Bank").Element("Branch Address").Value = hostingUnit1.Host.Bank.BranchAddress;
            }

            catch
            {
                throw new objectErrorDAL();//didn't find item
            }
            try
            {
                hostingUnits.Save(hostingUnitPath);//saves
            }
            catch
            {
                throw new loadExceptionDAL("unable to save elements after deleting");//error in loading or saving the file
            }
        }

        public void addHostingUnit(HostingUnit unit)//adds unit to xelement and to xml file
        {
            try
            {
                #region unit details
                //checks if exists
                var u= (from hostingUnit in hostingUnits.Elements()
                where Convert.ToInt32(hostingUnit.Element("Unit Key").Value) == unit.HostingUnitKey
                select hostingUnit).First();//first of units found with this key
                if (u!=null)//unit already exists
                   throw new duplicateErrorDAL();
                //otherwise creates xelement of unit and adds it
                XElement unitKey = new XElement("Unit Key", unit.HostingUnitKey);
                XElement unitName = new XElement("Unit Name", unit.HostingUnitName);
                XElement unitType = new XElement("Unit Name", unit.HostingUnitType);
                XElement unitArea = new XElement("Unit Area", unit.AreaVacation);
                XElement adults = new XElement("Adults", unit.NumAdult);
                XElement child = new XElement("Children", unit.NumChildren);
                XElement pool = new XElement("Pool", unit.Pool);
                XElement garden = new XElement("Garden", unit.Garden);
                XElement j = new XElement("Jacuzzi", unit.Jacuzzi);
                XElement meals = new XElement("Meal", unit.Meal);
                XElement paid = new XElement("Paid", unit.MoneyPaid);
                #endregion
                #region host and bank
                //host
                XElement hostKey = new XElement("Paid", unit.Host.HostKey);
                XElement hostFirst = new XElement("Paid", unit.Host.Name);
                XElement hostLast = new XElement("Paid", unit.Host.LastName);
                XElement mail = new XElement("Paid", unit.Host.Mail.Address);
                XElement clearance = new XElement("Clearance", unit.Host.CollectionClearance);

                //bank
                XElement account = new XElement("Account Number", unit.Host.Bank.BankAcountNumber);
                XElement bankName = new XElement("Bank Name", unit.Host.Bank.BankName);
                XElement bankNumber = new XElement("Bank Number", unit.Host.Bank.BankNumber);
                XElement branchNumber = new XElement("Branch Number", unit.Host.Bank.BranchNumber);
                XElement branchAddress = new XElement("Branch Address", unit.Host.Bank.BranchAddress);
                XElement bank = new XElement("Bank", account, bankName, bankNumber, branchNumber, branchAddress);

                XElement host = new XElement("Host", hostKey, hostFirst, hostLast, mail, clearance, bank);
                #endregion
                hostingUnits.Add(new XElement("Hosting Unit", unitKey, unitName, unitType, unitArea, adults, child, pool, garden, j, meals, paid, host));
            }
            catch(Exception ex)
            {
                if (ex is duplicateErrorDAL)
                    throw ex;
                //otherwise throws a new exception
                throw new loadExceptionDAL("unable to save new unit to xml file");
            }
        }

        #endregion
        #region not implemented functions from IDAL


        public void addGuest(GuestRequest guest)
        {
            throw new NotImplementedException();
        }

        public GuestRequest findGuest(int g1)
        {
            throw new NotImplementedException();
        }

        public GuestRequest findGuest(GuestRequest g1)
        {
            throw new NotImplementedException();
        }

        public List<GuestRequest> getRequests()
        {
            throw new NotImplementedException();
        }

        public void changeStatus(GuestRequest guest, Enums.OrderStatus status)
        {
            throw new NotImplementedException();
        }

        public List<Order> getAllOrders()
        {
            throw new NotImplementedException();
        }

        public void addOrder(Order ord)
        {
            throw new NotImplementedException();
        }

        public void changeOrder(Func<Order, bool> p1, Func<Order, Order> p2)
        {
            throw new NotImplementedException();
        }

        public void addCharge(HostingUnit unit, int numDays)
        {
            throw new NotImplementedException();
        }

        public void deleteOrders(Func<Order, bool> p)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
