var builder = WebApplication.CreateBuilder(args);

// Añade los servicios de controladores y Swagger.
// Esto es necesario para que tus controladores y la interfaz de Swagger funcionen.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configura el pipeline de la petición HTTP.
if (app.Environment.IsDevelopment())
{
    // Habilita la interfaz de Swagger UI solo en modo de desarrollo.
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Habilita el ruteo. Las peticiones HTTP ahora pueden dirigirse a tus controladores.
app.UseRouting();

// Habilita la autorización y autenticación, aunque el código "malo" no lo use completamente
app.UseAuthorization();

// Mapea los controladores para que el programa sepa cómo conectar las rutas.
app.MapControllers();

app.Run();