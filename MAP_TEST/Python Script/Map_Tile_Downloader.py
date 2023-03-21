import math
import sqlite3
import requests
from tqdm import tqdm, trange

# Set the parameters for the tile downloader
url_pattern = 'https://api.mapbox.com/v4/mapbox.satellite/{}/{}/{}.jpg?access_token=pk.eyJ1IjoiaGxzdHJrIiwiYSI6ImNsZmEwdzE2YTBkZWwzcG11MGp6bmZoNTQifQ.MP9aaEJ_WbRi-d3cbDQ2Dw'
zoom_levels = [19,17,15,14,16, 18, 20]
coord_lat, coord_lon = 40.799565595801035,30.29116657739119
tile_radius = 10  # in tiles, equivalent to 20km

# Replace YOUR_ACCESS_TOKEN with your Mapbox access token

# Connect to the SQLite database
conn = sqlite3.connect('mydatabase.sqlite')

# Create the maptiles table if it doesn't exist
conn.execute('''CREATE TABLE IF NOT EXISTS maptiles
                 (zoom_level INTEGER, tile_column INTEGER, tile_row INTEGER, tile_data BLOB,
                 PRIMARY KEY (zoom_level, tile_column, tile_row))''')

total_tiles = sum([(2 * tile_radius + 1) ** 2 for tile_radius in range(tile_radius)])
tiles_downloaded = 0

# Download and store the tiles in the database
for zoom in zoom_levels:
    # Calculate the tile coordinates for the center of the square
    tile_x, tile_y = math.floor((coord_lon + 180) / 360 * 2**zoom), math.floor((1 - math.log(math.tan(math.radians(coord_lat)) + 1 / math.cos(math.radians(coord_lat))) / math.pi) / 2 * 2**zoom)

    # Calculate the range of tiles to download
    x_min, x_max = tile_x - tile_radius, tile_x + tile_radius
    y_min, y_max = tile_y - tile_radius, tile_y + tile_radius

    with trange(x_min, x_max+1, desc=f"Zoom {zoom}") as x_range:
        for x in x_range:
            for y in range(y_min, y_max+1):
                # Check if the tile is within the specified radius
                if math.sqrt((x - tile_x)**2 + (y - tile_y)**2) <= tile_radius:
                    tile_url = url_pattern.format(zoom, x, y)
                    try:
                        response = requests.get(tile_url)
                        if response.status_code == 200:
                            tile_data = response.content
                            conn.execute('''INSERT OR REPLACE INTO maptiles
                                             (zoom_level, tile_column, tile_row, tile_data)
                                             VALUES (?, ?, ?, ?)''',
                                         (zoom, x, y, tile_data))
                            tiles_downloaded += 1
                            x_range.set_postfix({'tiles_downloaded': tiles_downloaded})
                    except:
                        print(response.status_code)

# Commit the changes and close the connection
conn.commit()
conn.close()

with tqdm(total=total_tiles, desc="Total Progress") as pbar:
    pbar.update(tiles_downloaded)
