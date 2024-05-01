# BotPrecios

BotPrecios es un proyecto que nace a partir de la necesidad de entender la variación de precios de la Canasta Básica Alimentaria (CBA) en Argentina, tomando la mayor cantidad de variables posibles, consultando precios en diferentes cadenas de supermercados.

Para la obtención de datos se utiliza Sellenium, una herramienta para obtener datos de cualquier sitio web. Estos datos son almacenados en una base de datos SQLite, para posteriormente ser consultados y expuestos.

Luego, se procede a utilizar una API para el posteo de los resultados en X.




## Funcionamiento

**Obtención de datos:** Los datos se obtienen mediante scraping en las páginas webs de los supermercados cargados.

**Almacenamiento de datos:** Se almacenan en una base de datos simple de SQLite de manera local.

**Obtención de estadísticas:** Las estadísitcas se obtienen mediante la base de datos. Son comparados los precios obtenidos en el día con los del primer día del mes, de esa manera se calcula la variación de los precios durante el corriente. Además se calcula el producto con más variación positiva y con más variación negativa, es decir, el que más bajó de precio y el que más subió respectivamente. Para este último cálculo, se tienen en cuenta los productos que tienen un precio cargado, ya que muchas veces, si no hay stock, no se puede obtener el precio actual del producto. Por último, cada fin de mes, se obtendrá el supermercado que tiene la canasta básica más barata y el que tiene la más cara.

Cabe aclarar que los productos que se utilizan para calcular la canasta básica son seleccionados para tratar de comparar los mismos productos entre los diferentes supermercados. Estos son almacenados en un JSON por supermercado.

## Disclaimer

Los datos obtenidos por el bot, no deben ser tomados como un reflejo de la realidad. Este programa fue realizado con el fin de obtener una medición estimativa y no oficial, por lo tanto no debe ser utilizado con fines estadísticos.

