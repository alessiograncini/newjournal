import { Hono } from 'https://deno.land/x/hono@v4.2.2/mod.ts'
import Anthropic from 'npm:@anthropic-ai/sdk'
import { load } from 'https://deno.land/std@0.221.0/dotenv/mod.ts'

const env = await load()
const ANTHROPIC_API_KEY = ''
const PASSWORD = ''

const app = new Hono()

const anthropic = new Anthropic({
    apiKey: ANTHROPIC_API_KEY,
})

const getState = async (
    prompt: string
): Promise<{
    prompt: string
    state: string
}> => {
    const msg = await anthropic.messages.create({
        model: 'claude-3-haiku-20240307',
        max_tokens: 40,
        temperature: 0.7,
        system: 'given a prompt from user just categorise it as either past and future. just reply with the word, nothing else.\n\neg. \nI want to walk in SF daily - future\nI think I did bad in my exam yesterday - past\n',
        messages: [
            {
                role: 'user',
                content: [
                    {
                        type: 'text',
                        text: prompt,
                    },
                ],
            },
        ],
    })
    const stringState: string = msg.content[0].text as string
    return {
        prompt,
        state: stringState,
    }
}

app.get('/', async (c) => {
    // read c and get header Authorization
    // const auth = c.req.header()['authorization']
    // if (auth !== PASSWORD) {
    //     return c.json({ error: 'Unauthorized' })
    // }
    const url = new URL(c.req.url)
    const params = url.searchParams
    const prompt = params.get('prompt')
    const [msg, state] = await Promise.all([
        anthropic.messages.create({
            model: 'claude-3-haiku-20240307',
            max_tokens: 2000,
            temperature: 0.7,
            system: 'your job is to give 3 solutions for a given task by user, give me only the 3 solutions 1., 2., 3. just them in plain text, no explanations\n\neach prompt should be only max 25 words',
            messages: [
                {
                    role: 'user',
                    content: [
                        {
                            type: 'text',
                            text: 'I want to walk in SF daily ',
                        },
                    ],
                },
            ],
        }),
        getState(prompt!),
    ])
    const all = msg.content[0].text
    const splitByNewLine = all.split('\n')
    const removeFirst3LettersOnEachLine = splitByNewLine.map((line) =>
        line.slice(3)
    )
    const promises = removeFirst3LettersOnEachLine.map((line) => {
        return {
            prompt: line,
            state,
        }
    })
    console.log(promises)
    return c.json(promises)
})

Deno.serve(app.fetch)
