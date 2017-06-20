using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using SC.DAL;
using SC.BL.Domain;
using System.IO;
using static System.Console;

namespace SC.BL
{
  public class TicketManager : ITicketManager
  {
    private readonly ITicketRepository repo;
        
    public TicketManager()
    {
            /*bool exists = false;
            try
            {
                var asm = Assembly.GetEntryAssembly();
                string filename = Path.Combine(new FileInfo(asm.Location).DirectoryName, "SC.Repo.dll");
                exists = File.Exists(filename);
                Assembly asmRepo = Assembly.LoadFile(filename);
                Type t = asmRepo.GetType("SC.Repo.TicketRepository");
                repo = (ITicketRepository)Activator.CreateInstance(t);
            } 
            catch (FileNotFoundException ex) when (!exists)
            {
                Console.WriteLine("File does not exist!");
                Console.WriteLine(ex.Message);
            }*/

           repo = TicketFactory<DAL.EF.TicketRepository, ITicketRepository>.Create();
        }

    public IEnumerable<Ticket> GetTickets()
    {
      return repo.ReadTickets();
    }

    public Ticket GetTicket(int ticketNumber)
    {
      return repo.ReadTicket(ticketNumber);
    }

    public Ticket AddTicket(int accountId, string question)
    {
      Ticket t = new Ticket()
      {
        AccountId = accountId,
        Text = question,
        DateOpened = DateTime.Now,
        State = TicketState.Open,
      };
      return this.AddTicket(t);
    }

    public Ticket AddTicket(int accountId, string device, string problem)
    {
      

      try
            {
                if (accountId == 0)
                {
                    throw new Exception("1");
                } else if (problem == null)
                {
                    throw new Exception("2");
                } else if (device == null)
                {
                    throw new Exception("3");
                }
                Ticket t = new HardwareTicket()
                {
                    AccountId = accountId,
                    Text = problem,
                    DateOpened = DateTime.Now,
                    State = TicketState.Open,
                    DeviceName = device
                };
                return this.AddTicket(t);
            } catch (Exception ex) when (ex.Message == "1")
            {
                WriteLine();
                WriteLine(nameof(accountId) + " is niet ingevuld.");
                WriteLine();
            } catch (Exception ex) when (ex.Message == "2")
            {
                WriteLine();
                WriteLine(nameof(problem) + " is niet ingevuld.");
                WriteLine();
            } catch (Exception ex) when (ex.Message == "3")
            {
                WriteLine();
                WriteLine(nameof(device) + " is niet ingevuld.");
                WriteLine();
            }
            return null;
            
      
    }



    public Ticket AddTicket(Ticket ticket)
    {
      this.Validate<Ticket>(ticket);
      return repo.CreateTicket(ticket);
    }

    public void ChangeTicket(Ticket ticket)
    {
      this.Validate<Ticket>(ticket);
      repo.UpdateTicket(ticket);
    }

    public void RemoveTicket(int ticketNumber)
    {
      repo.DeleteTicket(ticketNumber);
    }

    public IEnumerable<TicketResponse> GetTicketResponses(int ticketNumber)
    {
      return repo.ReadTicketResponsesOfTicket(ticketNumber);
    }

    public TicketResponse AddTicketResponse(int ticketNumber, string response, bool isClientResponse)
    {
      Ticket ticketToAddResponseTo = this.GetTicket(ticketNumber);
      if (ticketToAddResponseTo != null)
      {
        // Create response
        TicketResponse newTicketResponse = new TicketResponse();
        newTicketResponse.Date = DateTime.Now;
        newTicketResponse.Text = response;
        newTicketResponse.IsClientResponse = isClientResponse;
        newTicketResponse.Ticket = ticketToAddResponseTo;

        // Add response to ticket
        var responses = this.GetTicketResponses(ticketNumber);
        if (responses != null)
          ticketToAddResponseTo.Responses = responses.ToList();
        else
          ticketToAddResponseTo.Responses = new List<TicketResponse>();
        ticketToAddResponseTo.Responses.Add(newTicketResponse);

        // Change state of ticket
        if (isClientResponse)
          ticketToAddResponseTo.State = TicketState.ClientAnswer;
        else
          ticketToAddResponseTo.State = TicketState.Answered;


        // Validatie van ticketResponse en ticket afdwingen!!!
        this.Validate<TicketResponse>(newTicketResponse);
        this.Validate<Ticket>(ticketToAddResponseTo);

        // Bewaren naar db
        repo.CreateTicketResponse(newTicketResponse);
        repo.UpdateTicket(ticketToAddResponseTo);

        return newTicketResponse;
      }
      else
        throw new ArgumentException("Ticketnumber '" + ticketNumber + "' not found!");
    }

    public void ChangeTicketStateToClosed(int ticketNumber)
    {
      repo.UpdateTicketStateToClosed(ticketNumber);
    }

    private void Validate<T>(T t)
    {
      //Validator.ValidateObject(t, new ValidationContext(t), validateAllProperties: true);

      List<ValidationResult> errors = new List<ValidationResult>();
      bool valid = Validator.TryValidateObject(t, new ValidationContext(t), errors, validateAllProperties: true);

            if (!valid)
                throw new ValidationException(t.GetType().Name + " not valid!");
            else Console.WriteLine(t.GetType().Name + " is valid!");
    }
  }
}
