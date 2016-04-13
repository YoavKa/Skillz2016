import os
import zipfile

relative_src = '.'
dst_name = 'PoodleChanBD'

bad_directories = ['bin', 'obj', 'properties', '.git', '.vs']

def zip(src, dst):
    zf = zipfile.ZipFile('%s.zip' % (dst), 'w', zipfile.ZIP_DEFLATED)
    abs_src = os.path.abspath(src)
    total_length = 0
    total_files = 0
    for dirname, subdirs, files in os.walk(src):
        testdir = dirname[2:]
        if testdir.split('\\')[0].lower() in bad_directories:
            continue
        print_directory = False
        for filename in files:
            if not filename.endswith('.cs'):
                continue
            total_files += 1
            absname = os.path.abspath(os.path.join(dirname, filename))
            arcname = absname[len(abs_src) + 1:]
            with open(os.path.join(dirname, filename)) as f:
                len_code = len(f.readlines())
                total_length += len_code
            if not print_directory:
                print 'Directory: %s ' % (dirname)
                print_directory = True
            print '-- File %s with %s lines of code' % (filename,
                                        len_code)
            zf.write(absname, arcname)
    zf.close()
    print 'There are %s lines in %s files' % (total_length, total_files)


zip(relative_src, dst_name)
raw_input('<Press Enter to Exit>')
