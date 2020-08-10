import setuptools

with open('README.md') as f:
    README = f.read()

setuptools.setup(
    author="Griffin Teller",
    author_email="griffinteller@gmail.com",
    name='awconnection',
    license="MIT",
    description='An interface between Python and AutonoWar',
    version='v0.3.5',
    long_description=README,
    url='https://github.com/griffinteller/awconnection',
    packages=setuptools.find_packages(),
    python_requires=">=3.6",
    install_requires=[

        "pywin32==227;platform_system=='Windows'"

    ],
    classifiers=[

        'Development Status :: 2 - Pre-Alpha',
        'License :: OSI Approved :: MIT License',
        'Programming Language :: Python :: 3',
        'Topic :: Software Development :: Libraries',
        'Topic :: Software Development :: Libraries :: Python Modules',
        'Intended Audience :: Education',

    ],
)
