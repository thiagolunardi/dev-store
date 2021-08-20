using DevStore.Clients.API.Application.Events;
using DevStore.Clients.API.Models;
using DevStore.Core.Messages;
using FluentValidation.Results;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace DevStore.Clients.API.Application.Commands
{
    public class ClientCommandHandler : CommandHandler,
        IRequestHandler<NewClientCommand, ValidationResult>,
        IRequestHandler<AddAddressCommand, ValidationResult>
    {
        private readonly IClienteRepository _clienteRepository;

        public ClientCommandHandler(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<ValidationResult> Handle(NewClientCommand message, CancellationToken cancellationToken)
        {
            if (!message.IsValid()) return message.ValidationResult;

            var client = new Client(message.Id, message.Name, message.Email, message.SocialNumber);

            var clientExist = await _clienteRepository.GetBySocialNumber(client.SocialNumber);

            if (clientExist != null)
            {
                AddError("Already has this social number.");
                return ValidationResult;
            }

            _clienteRepository.Add(client);

            client.AddEvent(new NewClientAddedEvent(message.Id, message.Name, message.Email, message.SocialNumber));

            return await PersistData(_clienteRepository.UnitOfWork);
        }

        public async Task<ValidationResult> Handle(AddAddressCommand message, CancellationToken cancellationToken)
        {
            if (!message.IsValid()) return message.ValidationResult;

            var endereco = new Address(message.StreetAddress, message.BuildingNumber, message.SecondaryAddress, message.Neighborhood, message.ZipCode, message.City, message.State, message.ClientId);
            _clienteRepository.AddAddress(endereco);

            return await PersistData(_clienteRepository.UnitOfWork);
        }
    }
}