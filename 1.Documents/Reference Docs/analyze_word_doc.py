import win32com.client
import os

def analyze_word_doc(filename):
    """Analyze old Word document using COM automation"""
    word = None
    doc = None
    try:
        word = win32com.client.Dispatch("Word.Application")
        word.Visible = False

        doc_path = os.path.abspath(filename)
        doc = word.Documents.Open(doc_path, ReadOnly=True)

        print(f"Document: {filename}")
        print(f"Paragraphs: {doc.Paragraphs.Count}")
        print(f"Tables: {doc.Tables.Count}")
        print("\n" + "="*80)
        print("FIRST 50 PARAGRAPHS:")
        print("="*80)

        for i in range(1, min(51, doc.Paragraphs.Count + 1)):
            text = doc.Paragraphs(i).Range.Text.strip()
            if text:
                print(f"\nParagraph {i}: {text[:200]}")

        print("\n" + "="*80)
        print("TABLES STRUCTURE:")
        print("="*80)

        for i in range(1, min(6, doc.Tables.Count + 1)):
            table = doc.Tables(i)
            print(f"\nTable {i}:")
            print(f"  Rows: {table.Rows.Count}")
            print(f"  Columns: {table.Columns.Count}")
            print("  Sample data (first 10 rows):")

            for row_idx in range(1, min(11, table.Rows.Count + 1)):
                row_data = []
                for col_idx in range(1, min(table.Columns.Count + 1, 10)):
                    try:
                        cell_text = table.Cell(row_idx, col_idx).Range.Text.strip()
                        row_data.append(cell_text[:50])
                    except:
                        row_data.append("[merged/error]")
                print(f"    Row {row_idx}: {row_data}")

    except Exception as e:
        print(f"Error: {e}")
        import traceback
        traceback.print_exc()
    finally:
        if doc:
            doc.Close(False)
        if word:
            word.Quit()

# Analyze the Word document
word_file = "KTDA Factory ICT Report -  TBESONIK  MONTHLY - September 2025.doc"
analyze_word_doc(word_file)
