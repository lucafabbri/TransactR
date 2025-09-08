using Concordia;
using ConcordiaAndTransactor.Sample;
using ConcordiaAndTransactor.Sample.Application;
using ConcordiaAndTransactor.Sample.Contexts;
using ConcordiaAndTransactor.Sample.Domain;
using ConcordiaAndTransactor.Sample.Infrastructure;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using TransactR;
using TransactR.Concordia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Aggiunge i servizi per la generazione della specifica OpenAPI (Swagger)
// Assicurati di avere il pacchetto Swashbuckle.AspNetCore installato
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddSingleton<ICounterRepository, CounterRepository>();

builder.Services.AddConcordiaCoreServices();
builder.Services.AddCounterCommands();

builder.Services
    .AddTransactR()
    .OnConcordia()
        .HasState<CounterState>()
            .PersistedInMemory()
            .RestoredBy<EmptyStateRestorer<CounterState>>()
            .UseContext<CountContext>()
                .Surround<IncrementCommand>()
                .Surround<DecrementCommand>()
                .Surround<ResetCommand>();

var app = builder.Build();

// Configura la pipeline delle richieste HTTP.
if (app.Environment.IsDevelopment())
{
    // Abilita il middleware per generare il file swagger.json
    app.UseSwagger();

    // Abilita il middleware per servire la UI di Swagger
    app.UseSwaggerUI(options =>
    {
        // Imposta la UI di Swagger come pagina principale dell'applicazione
        options.RoutePrefix = string.Empty;
        // Specifica l'endpoint del file swagger.json
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapPost("api/counter", async ([FromServices] ISender sender) =>
{
    return Results.Ok(await sender.Send(new CreateCounterCommand()));
});

app.MapGet("api/counter", async (string id, [FromServices] ISender sender) =>
{
    return Results.Ok(await sender.Send(new GetAllCountersQuery()));
});

app.MapGet("api/counter/{id}", async (string id, [FromServices] ISender sender) =>
{
    return Results.Ok(await sender.Send(new GetCounterQuery(id)));
});

app.MapPatch("api/counter/{id}/increment", async (string id, [FromServices] ISender sender) =>
{
    var transactionId = Guid.NewGuid().ToString();
    var command = new IncrementCommand(id, transactionId);
    return Results.Ok(await sender.Send(command));
}); 

app.MapPatch("api/counter/{id}/decrement", async (string id, [FromServices] ISender sender) =>
{
    var transactionId = Guid.NewGuid().ToString();
    var command = new DecrementCommand(id, transactionId);
    return Results.Ok(await sender.Send(command));
});

app.MapPatch("api/counter/{id}/reset", async (string id, [FromServices] ISender sender) =>
{
    var transactionId = Guid.NewGuid().ToString();
    var command = new ResetCommand(id, transactionId);
    return Results.Ok(await sender.Send(command));
});

app.Run();
