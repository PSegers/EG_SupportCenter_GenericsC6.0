using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SC.BL;
using SC.BL.Domain;
using SC.UI.CA.ExtensionMethods;
using static System.Console;
using System.Reflection;
using System.IO;

namespace SC.UI.CA
{
  class Program
  {
    private static bool quit = false;
        private static ITicketManager mgr = new TicketManager();

        //private static readonly Service srv = new Service();

        static void Main(string[] args)
    {
            

            while (!quit)
        ShowMenu();
    }

    private static void ShowMenu()
    {
      WriteLine("=================================");
      WriteLine("=== HELPDESK - SUPPORT CENTER ===");
      WriteLine("=================================");
      WriteLine("1) Toon alle tickets");
      WriteLine("2) Toon details van een ticket");
      WriteLine("3) Toon de antwoorden van een ticket");
      WriteLine("4) Maak een nieuw ticket");
      WriteLine("5) Geef een antwoord op een ticket");
      WriteLine("6) Markeer ticket als 'Closed'");
      WriteLine("0) Afsluiten");
      try
      {
        DetectMenuAction();
      }
      catch (Exception ex)
      {
        WriteLine(ex.Message);
      }
    }
    
    private static void DetectMenuAction()
    {
      bool inValidAction = false;
      do
      {
        Write("Keuze: ");
        string input = ReadLine();
        int action;
        if (Int32.TryParse(input, out action))
        {
          switch (action)
          {
            case 1:
              PrintAllTickets(); break;
            case 2:
              ActionShowTicketDetails(); break;
            case 3:
              ActionShowTicketResponses(); break;
            case 4:
              ActionCreateTicket(); break;
            case 5:
              ActionAddResponseToTicket(); break;
            case 6:
              ActionCloseTicket(); break;
            case 0:
              quit = true;
              return;
            default:
              WriteLine("Geen geldige keuze!");
              inValidAction = true;
              break;
          }
        }
      } while (inValidAction);
    }

    private static void ActionCloseTicket()
    {
      Write("Ticketnummer: ");
      int input = Int32.Parse(ReadLine());

      mgr.ChangeTicketStateToClosed(input);
      // via WebAPI-service
      //srv.ChangeTicketStateToClosed(input);
    }

    private static void PrintAllTickets()
    {
      foreach (var t in mgr.GetTickets())
            {
                WriteLine(t.GetInfo());
            }
        
    }

    private static void ActionShowTicketDetails()
    {
      Write("Ticketnummer: ");
      int input = Int32.Parse(ReadLine());

      Ticket t = mgr.GetTicket(input);
      PrintTicketDetails(t);
    }

    private static void PrintTicketDetails(Ticket ticket)
    {
            PropertyInfo[] propInfo;
            Type t = typeof(Ticket);
            propInfo = t.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            
      WriteLine($"{propInfo[0].Name,-15}: {ticket.TicketNumber}");
      WriteLine($"{propInfo[1].Name,-15}: {ticket.AccountId}");
      WriteLine($"{propInfo[3].Name,-15}: {ticket.DateOpened.ToString("dd/MM/yyyy")}");
      WriteLine($"{propInfo[4].Name,-15}: {ticket.State}");

      if (ticket is HardwareTicket)
        WriteLine($"{"Device",-15}: {((HardwareTicket)ticket).DeviceName}");

      WriteLine($"{propInfo[2].Name,-15}: {ticket.Text}");
    }

    private static void ActionShowTicketResponses()
    {
      Write("Ticketnummer: ");
      int input = Int32.Parse(ReadLine());

      IEnumerable<TicketResponse> responses = mgr.GetTicketResponses(input);
      // via Web API-service
      //IEnumerable<TicketResponse> responses = srv.GetTicketResponses(input);
      
      PrintTicketResponses(responses);
    }

    private static void PrintTicketResponses(IEnumerable<TicketResponse> responses)
    {
      foreach (var r in responses)
        WriteLine(r?.GetInfo() ?? "Er zijn geen responses!");
    }

    private static void ActionCreateTicket()
    {
      int accountNumber = 0;
      string problem = "";
      string device = "";
      
      Write("Is het een hardware probleem (j/n)? ");
      bool isHardwareProblem = (ReadLine().ToLower() == "j");
      if (isHardwareProblem)
      {
        Write("Naam van het toestel: ");
        device = ReadLine();
      }

      Write("Gebruikersnummer: ");
      accountNumber = Int32.Parse(ReadLine());
      Write("Probleem: ");
      problem = ReadLine();

      if (!isHardwareProblem)
        mgr.AddTicket(accountNumber, problem);
      else
        mgr.AddTicket(accountNumber, problem, device);
    }

    private static void ActionAddResponseToTicket()
    {
      Write("Ticketnummer: ");
      int ticketNumber = Int32.Parse(ReadLine());
      Write("Antwoord: ");
      string response = ReadLine();

      mgr.AddTicketResponse(ticketNumber, response, false);
      // via WebAPI-service
      //srv.AddTicketResponse(ticketNumber, response, false);
    }
  }
}