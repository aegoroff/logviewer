/*!
 * \brief   The file contains Hash LINQ implementation
 * \author  \verbatim
            Created by: Alexander Egorov
            \endverbatim
 * \date    \verbatim
            Creation date: 2011-11-14
            \endverbatim
 * Copyright: (c) Alexander Egorov 2009-2013
 */

#include "targetver.h"
#include <locale.h>
#include "rdr.h"
#include "argtable2.h"
#include "apr.h"
#include "apr_pools.h"
#include "apr_strings.h"
#include "apr_file_io.h"
#include "pcre.h"

#ifdef WIN32
#include "..\srclib\DebugHelplers.h"
#endif

#define ERROR_BUFFER_SIZE 2 * BINARY_THOUSAND
#define LINE_FEED '\n'

#define PATH_ELT_SEPARATOR '\\'
#define NUMBER_PARAM_FMT_STRING "%lu"
#define BIG_NUMBER_PARAM_FMT_STRING "%llu"
#define START_MSG "^\\s*\\[?\\d{4}-\\d{2}-\\d{2}\\s+\\d{2}:\\d{2}:\\d{2}([,.]\\d{3,})?.*"

#define MAX_LINE_SIZE 32 * BINARY_THOUSAND - 1
#define MAX_STRING_LEN 2048 * 2

apr_pool_t* pcrePool = NULL;

void PrintError(apr_status_t status)
{
    char errbuf[ERROR_BUFFER_SIZE];
    apr_strerror(status, errbuf, ERROR_BUFFER_SIZE);
    CrtPrintf("%s", errbuf); //-V111
    NewLine();
}

void* PcreAlloc(size_t size)
{
    return apr_palloc(pcrePool, size);
}

int main(int argc, const char* const argv[])
{
    apr_pool_t* pool = NULL;
    apr_status_t status = APR_SUCCESS;
    int nerrors;

    pcre* re = NULL;
    const char* error = NULL;
    int erroffset = 0;
    int rc = 0;
    int flags  = PCRE_NOTEMPTY;

    apr_file_t* fileHandle = NULL;
    char* line = NULL;
    long long messages = 0;

    struct arg_file *file          = arg_file0("f", "file", NULL, "full path to log file");
    struct arg_lit  *help          = arg_lit0("h", "help", "print this help and exit");
    struct arg_end  *end           = arg_end(10);

    void* argtable[] = { file, help, end };

#ifdef WIN32
#ifndef _DEBUG  // only Release configuration dump generating
    SetUnhandledExceptionFilter(TopLevelFilter);
#endif
#endif

    setlocale(LC_ALL, ".ACP");
    setlocale(LC_NUMERIC, "C");

    pcre_malloc = PcreAlloc;

    status = apr_app_initialize(&argc, &argv, NULL);
    if (status != APR_SUCCESS) {
        CrtPrintf("Couldn't initialize APR");
        NewLine();
        PrintError(status);
        return EXIT_FAILURE;
    }
    atexit(apr_terminate);
    apr_pool_create(&pool, NULL);

    if (arg_nullcheck(argtable) != 0) {
        PrintSyntax(argtable);
        goto cleanup;
    }

    /* Parse the command line as defined by argtable[] */
    nerrors = arg_parse(argc, argv, argtable);

    if (help->count > 0) {
        PrintSyntax(argtable);
        goto cleanup;
    }
    if (nerrors > 0 || argc < 2) {
        arg_print_errors(stdout, end, PROGRAM_NAME);
        PrintSyntax(argtable);
        goto cleanup;
    }

    pcrePool = pool; // needed for pcre_alloc (PcreAlloc) function

    re = pcre_compile(START_MSG,           /* the pattern */
                      PCRE_UTF8,
                      &error,          /* for error message */
                      &erroffset,      /* for error offset */
                      0);
    if (!re) {
        CrtPrintf("%s", error); //-V111
        goto cleanup;
    }

    status = apr_file_open(&fileHandle, file->filename[0], APR_FOPEN_READ | APR_FOPEN_BUFFERED, APR_FPROT_WREAD, pool);
    if (status != APR_SUCCESS) {
        PrintError(status);
        goto cleanup;
    }
    line = (char*)apr_pcalloc(pool, MAX_STRING_LEN);

    while (status != APR_EOF) {
        status = apr_file_gets(line, MAX_STRING_LEN, fileHandle);
        rc = pcre_exec(
            re,                   /* the compiled pattern */
            0,                    /* no extra data - pattern was not studied */
            line,                  /* the string to match */
            (int)strlen(line),     /* the length of the string */
            0,                    /* start at offset 0 in the subject */
            flags,
            NULL,              /* output vector for substring information */
            0);           /* number of elements in the output vector */
        if (rc >= 0) {
            ++messages;
        }
    }
    CrtPrintf(BIG_NUMBER_PARAM_FMT_STRING, messages);

cleanup:
    /* deallocate each non-null entry in argtable[] */
    arg_freetable(argtable, sizeof(argtable) / sizeof(argtable[0]));
    apr_pool_destroy(pool);
    return EXIT_SUCCESS;
}

void PrintSyntax(void* argtable) {
    PrintCopyright();
    arg_print_syntax(stdout, argtable, NEW_LINE NEW_LINE);
    arg_print_glossary_gnu(stdout,argtable);
}

void PrintCopyright(void)
{
    CrtPrintf(COPYRIGHT_FMT, APP_NAME);
}
