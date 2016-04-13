import random
printable = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!#$%&()*+,-./:;=?@^_`|~'
letters = [i for i in printable]
random.shuffle(letters)
letters = [a for a in letters]
printable = [a for a in printable]

str_a = '{'
str_b = '{'

for encrypted, printable in zip(letters, printable):
    str_a += "'" + str(encrypted) + "':'" + str(printable) + "', "
    str_b += "{'" + str(printable) + "','" + str(encrypted) + "'}, "
    if str(encrypted) == '\"':
        print hex(encrypted)

str_a = str_a[:-2] + '}'
str_b = str_b[:-2] + '}'

print str_a
print str_b
raw_input('<Press Enter to Exit>')
