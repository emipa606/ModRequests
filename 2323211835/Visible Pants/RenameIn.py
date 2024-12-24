import os

root_dir = './Textures' + '_out'

for directory, subdirectories, files in os.walk(root_dir):
    for file in files:
        fileName = os.path.join(directory, file).replace('\\', '/')
        fileDirectory = fileName.replace('_out', '').replace('@', '/')
        print(fileName)
        print(fileDirectory)
        os.rename(fileName, fileDirectory)

os.rmdir(root_dir)