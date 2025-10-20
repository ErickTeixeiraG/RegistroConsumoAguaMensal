using ERICK_TEIXEIRA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

// func 1
app.MapPost("/api/consumo/cadastrar", ([FromBody] RegistroConsumo registro, [FromServices] AppDataContext ctx) =>
{
    // Validacoes
    if (registro.mes < 1 || registro.mes > 12)
    {
        return Results.BadRequest("O mês inserido é inválido");
    }
    if (registro.ano <= 2000)
    {
        return Results.BadRequest("O ano inserido é inválido");
    }
    if (registro.m3Consumidos < 0)
    {
        return Results.BadRequest("A quantidade de m3 consumidos deve ser maior que 0");
    }
    if (ctx.RegistroConsumos.Any(r => r.cpf == registro.cpf && r.mes == registro.mes && r.ano == registro.ano))
    {
        return Results.BadRequest("Consumo já cadastrado");
    }
    if (registro.cpf == null)
    {
        return Results.BadRequest("O CPF não pode ficar em branco");
    }

    // calculos
    // validação consumo
    if (registro.m3Consumidos < 10)
    {
        registro.consumoFaturado = 10;
    }

    // cálculo tarifa
    switch (registro.tarifa)
    {
        case <= 10:
            registro.tarifa = 2.50;
            break;
        case <= 20:
            registro.tarifa = 3.50;
            break;
        case <= 50:
            registro.tarifa = 5.00;
            break;
        default:
            registro.tarifa = 6.50;
            break;
    }

    // calculo bandeira
    if (registro.bandeira == "Verde")
    {
        registro.adicionalBandeira = 0;
    }
    else if(registro.bandeira == "Amarela")
    {
        registro.adicionalBandeira = registro.valorAgua*0.10;
    }
    else if(registro.bandeira == "Vermelho")
    {
        registro.adicionalBandeira = registro.valorAgua*0.20;
    }

    // cálculo água
    registro.valorAgua = registro.tarifa * registro.m3Consumidos;

    //taxa esgoto
    if (registro.possuiEsgoto)
    {
        registro.taxaEsgoto = (registro.valorAgua + registro.adicionalBandeira) * 0.80;
    }
    else
    {
        registro.taxaEsgoto = 0;
    }

    // calculo total
    registro.total = registro.valorAgua + registro.adicionalBandeira + registro.taxaEsgoto;

    ctx.RegistroConsumos.Add(registro);
    ctx.SaveChanges();
    return Results.Ok("Consumo cadastrado");
});

// funcao 2
app.MapGet("/api/consumo/listar", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.RegistroConsumos.Any())
    {
        return Results.Ok(ctx.RegistroConsumos.ToList());
    }
    return Results.NotFound("Nenhum registro encontrado");
});

// funcao 3
app.MapGet("/api/consumo/buscar/{cpf}/{mes}/{ano}", ([FromRoute] string cpf, [FromRoute] int mes, [FromRoute] int ano, [FromServices] AppDataContext ctx) =>
{
    var registro = ctx.RegistroConsumos.FirstOrDefault(r => r.cpf == cpf && r.mes == mes && r.ano == ano);
    if (registro == null)
    {
        return Results.NotFound("Nenhum registro encontrado para esse cpf, mês e ano");
    }
    return Results.Ok(registro);
});

//func 4
app.MapDelete("/api/consumo/remover/{cpf}/{mes}/{ano}", ([FromRoute] string cpf, [FromRoute] int mes, [FromRoute] int ano, [FromServices] AppDataContext ctx) =>
{
    var registro = ctx.RegistroConsumos.FirstOrDefault(r => r.cpf == cpf && r.mes == mes && r.ano == ano);
    if (registro == null)
    {
        return Results.NotFound("Nenhum registro encontrado para esse cpf, mês e ano");
    }
    ctx.RegistroConsumos.Remove(registro);
    ctx.SaveChanges();
    return Results.Ok("Registro deletado com sucesso");
});

// func 5
app.MapGet("/api/consumo/total-geral", ([FromServices] AppDataContext ctx) =>
{ 
    if (ctx.RegistroConsumos.Any())
    {
        var totalGeral = ctx.RegistroConsumos.Sum(r => r.total);
        return Results.Ok("totalGeral: "+totalGeral);
    }
    return Results.NotFound("Nenhum registro encontrado");
});


app.Run();
