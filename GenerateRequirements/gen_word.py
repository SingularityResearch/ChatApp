import docx
from docx.shared import Pt, Inches
from docx.enum.text import WD_PARAGRAPH_ALIGNMENT
from docx.oxml.ns import qn
from docx.oxml import OxmlElement
import markdown
import re
import os

def add_bookmark(paragraph, bookmark_text, bookmark_name):
    run = paragraph.add_run()
    tag = run._r
    start = OxmlElement('w:bookmarkStart')
    start.set(qn('w:id'), '0')
    start.set(qn('w:name'), bookmark_name)
    tag.append(start)
    text = OxmlElement('w:t')
    text.text = bookmark_text
    run._r.append(text)
    end = OxmlElement('w:bookmarkEnd')
    end.set(qn('w:id'), '0')
    end.set(qn('w:name'), bookmark_name)
    tag.append(end)

def create_toc(doc):
    paragraph = doc.add_paragraph()
    run = paragraph.add_run()
    fldChar = OxmlElement('w:fldChar')
    fldChar.set(qn('w:fldCharType'), 'begin')
    instrText = OxmlElement('w:instrText')
    instrText.set(qn('xml:space'), 'preserve')
    instrText.text = 'TOC \\o "1-3" \\h \\z \\u'
    fldChar2 = OxmlElement('w:fldChar')
    fldChar2.set(qn('w:fldCharType'), 'separate')
    fldChar3 = OxmlElement('w:fldChar')
    fldChar3.set(qn('w:fldCharType'), 'end')
    run._r.append(fldChar)
    run._r.append(instrText)
    run._r.append(fldChar2)
    run._r.append(fldChar3)

def parse_markdown_to_docx(filepath, doc):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    lines = content.split('\n')
    in_code_block = False
    
    for line in lines:
        if line.startswith('```'):
            in_code_block = not in_code_block
            if in_code_block:
                 p = doc.add_paragraph()
                 p.style = 'Normal'
            continue
            
        if in_code_block:
             p = doc.add_paragraph(line)
             p.style = 'No Spacing'
             p.paragraph_format.left_indent = Inches(0.5)
             for run in p.runs:
                 run.font.name = 'Courier New'
                 run.font.size = Pt(9)
             continue

        if line.startswith('# '):
            doc.add_heading(line[2:].strip(), level=1)
        elif line.startswith('## '):
            doc.add_heading(line[3:].strip(), level=2)
        elif line.startswith('### '):
            doc.add_heading(line[4:].strip(), level=3)
        elif line.startswith('#### '):
            doc.add_heading(line[5:].strip(), level=4)
        elif line.startswith('- ') or line.startswith('* '):
             # Basic handling for bold text in list items
             p = doc.add_paragraph(style='List Bullet')
             text = line[2:].strip()
             parts = re.split(r'(\*\*.*?\*\*)', text)
             for part in parts:
                 if part.startswith('**') and part.endswith('**'):
                     p.add_run(part[2:-2]).bold = True
                 else:
                     p.add_run(part)
        elif line.strip() == '':
            doc.add_paragraph()
        else:
            # Handle inline bold in normal paragraphs
            p = doc.add_paragraph()
            parts = re.split(r'(\*\*.*?\*\*)', line)
            for part in parts:
                if part.startswith('**') and part.endswith('**'):
                    p.add_run(part[2:-2]).bold = True
                else:
                    p.add_run(part)


def build_solution_arch_doc():
    doc = docx.Document()
    
    # Title Page
    title = doc.add_heading('Solution Architecture Document', 0)
    title.alignment = WD_PARAGRAPH_ALIGNMENT.CENTER
    doc.add_paragraph('\n\n\n\n')
    
    # Table of Contents
    doc.add_heading('Table of Contents', level=1)
    create_toc(doc)
    doc.add_page_break()

    # Summary Section
    doc.add_heading('Summary', level=1)
    p = doc.add_paragraph("This document provides a comprehensive overview of the ChatApp solution architecture. It combines the project's README, functional and security requirements, and deployment requirements into a single consolidated reference.")
    doc.add_page_break()

    # Parse Files
    files = ['../README.md', '../requirements.md', '../DeploymentRequirements.md']
    for file in files:
        if os.path.exists(file):
            parse_markdown_to_docx(file, doc)
            doc.add_page_break()
            
    doc.save('../SolutionArchitecture.docx')
    print("Successfully generated SolutionArchitecture.docx in the project root")

if __name__ == "__main__":
    build_solution_arch_doc()
