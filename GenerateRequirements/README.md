# Generate Requirements

This directory contains the Python script used to automatically combine the project's Markdown documentation (`README.md`, `requirements.md`, and `DeploymentRequirements.md`) into a single consolidated Microsoft Word document (`SolutionArchitecture.docx`).

## Prerequisites

To run this script, you must have Python installed along with the required dependencies.

1. Ensure **Python 3.8+** is installed on your system.
2. Install the necessary Python packages by running the following command from this directory:

```bash
pip install python-docx markdown
```

## Running the Script

To generate the Solution Architecture document, simply execute the python script from within this `GenerateRequirements` folder:

```bash
python gen_word.py
```

### Output
The script will read the markdown files from the parent directory and generated a compiled Word Document.
The output file `SolutionArchitecture.docx` will be generated in the root directory of the repository (`../SolutionArchitecture.docx`).
