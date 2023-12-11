import sys

KEY = b'kda47y298uned'

def xor(data, key):
	
	output_str = []

	for i in range(len(data)):
		current = data[i]
		current_key = key[i % len(key)]
		output_str.append(current ^ current_key)
	return output_str

try:
    plaintext = open(sys.argv[1], "rb").read()
except:
    print("File argument needed! %s <raw payload file>" % sys.argv[0])
    sys.exit()


ciphertext = xor(plaintext, KEY)
newfilebytesarray = bytearray(ciphertext)
newfile = open("enc.bin", "wb")
newfile.write(newfilebytesarray)