import os

root_dir = './Textures'
os.makedirs(root_dir + '_out', exist_ok=True)

for directory, subdirectories, files in os.walk(root_dir):
    for file in files:
        fileDirectory = os.path.join(directory, file)
        fileName = fileDirectory.replace(root_dir + '\\', root_dir + '_out/').replace('\\', '@')
        fileDirectory = fileDirectory.replace('\\', '/')
        print(fileDirectory)
        print(fileName)
        os.rename(fileDirectory, fileName)