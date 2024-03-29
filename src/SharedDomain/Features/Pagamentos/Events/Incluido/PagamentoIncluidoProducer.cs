﻿using SharedDomain.Shared;

namespace SharedDomain.Features.Pagamentos.Events.Incluido;

public class PagamentoIncluidoProducer : EventProducer<PagamentoIncluidoEvent>
{
    public string TopicName => "pagamento-criado";

    public Task Send(PagamentoIncluidoEvent command, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
