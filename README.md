# Pausa

Recordatorios de descanso para los que pasamos el día frente a la pantalla.
Vive en la bandeja de Windows y tira un cartel a pantalla completa cuando toca parar.

Un solo .exe de 56 KB. Corre sobre el .NET Framework que ya trae Windows, no hay nada que instalar.

## Los tres ritmos

| Pausa | Cada | Dura | Para qué |
|---|---|---|---|
| Descanso ocular | 20 min | 20 s | Regla 20-20-20: mirar algo a más de 6 metros relaja el músculo ciliar |
| Levantarse | 30 min | 20 s | Cortar el tiempo sentado de corrido |
| Pausa larga | 60 min | 5 min | Rutina guiada: cuello, hombros, muñecas, cadera, ojos, agua |

Si la ocular y la de levantarse caen juntas se muestran como una sola pausa de dos pasos.
La pausa larga reinicia a las otras dos.

## Cómo se porta

- El cartel va por encima de todo, en todos los monitores, y no se cierra con Alt+F4.
- `Esc` pospone, `S` saltea. Los dos se pueden desactivar si querés algo más estricto.
- Si no tocás teclado ni mouse por unos minutos, el reloj se frena: ese rato ya fue descanso.
- Avisa 10 segundos antes con un cartelito en la esquina.
- Al volver de suspensión o de bloquear la sesión los contadores arrancan de cero.

### Juegos

Nadie quiere un cartel a pantalla completa en medio de una partida. Se le pasa una lista de
procesos y, mientras alguno esté abierto, no interrumpe nada y el reloj queda en cero.
Cuando el proceso se cierra, salta la pausa que corresponda al tiempo que duró.

Viene con `League of Legends` cargado, que es el proceso de la partida en sí (el cliente es otro
y no cuenta), así que la pausa te agarra en el lobby y no en un teamfight. Para cualquier otro juego
hay un botón que lista las apps abiertas y evita tener que adivinar el nombre del proceso.

## Compilar

```
powershell -ExecutionPolicy Bypass -File compilar.ps1
```

Usa el `csc.exe` del .NET Framework que viene con Windows, así que no hace falta Visual Studio.
Deja el ejecutable en `%LOCALAPPDATA%\Pausa` y lo arranca.

La configuración se guarda en `%APPDATA%\Pausa\pausa.ini` y se edita con doble clic en el ícono
de la bandeja. Para que arranque solo con Windows hay una opción en esa misma ventana.

## Los archivos

- `App.cs`: ícono de bandeja, contadores y cuándo dispara cada pausa
- `OverlayForm.cs`: el cartel a pantalla completa
- `AvisoForm.cs`: el aviso previo de la esquina
- `SettingsForm.cs` / `ElegirAppForm.cs`: configuración
- `Salud.cs`: ejercicios, variantes de descanso ocular y consejos
- `Config.cs`: el ini
- `Native.cs`: tiempo sin actividad y detección de pantalla completa
- `Icono.cs`: el ojo, dibujado en memoria
