// Offline renderer only: Unity ships the resulting WAVs, not this synthesizer.
#define TSF_IMPLEMENTATION
#include "vendor/tinysoundfont/tsf.h"
#include <algorithm>
#include <cstdio>
#include <fstream>
#include <vector>
struct Event { int sample, channel, key; float velocity; };
int main(int argc, char** argv) {
    if (argc != 4) return 1;
    tsf* synth = tsf_load_filename(argv[1]);
    if (!synth) return 2;
    std::ifstream score(argv[2]);
    int rate, length, channels, count;
    score >> rate >> length >> channels >> count;
    tsf_set_output(synth, TSF_STEREO_INTERLEAVED, rate, -10);
    tsf_set_max_voices(synth, 256);
    for (int ch = 0; ch < channels; ++ch) {
        int bank, preset; float pan, volume;
        score >> bank >> preset >> pan >> volume;
        if (!tsf_channel_set_bank_preset(synth, ch, bank, preset)) return 3;
        tsf_channel_set_pan(synth, ch, pan);
        tsf_channel_set_volume(synth, ch, volume);
    }
    std::vector<Event> events(count);
    for (auto& event : events) score >> event.sample >> event.channel >> event.key >> event.velocity;
    if (!score) return 4;
    FILE* output = fopen(argv[3], "wb");
    if (!output) return 5;
    int position = 0, index = 0; float buffer[2048];
    while (position < length) {
        while (index < count && events[index].sample <= position) {
            const auto& e = events[index++];
            if (e.velocity > 0) tsf_channel_note_on(synth, e.channel, e.key, e.velocity);
            else tsf_channel_note_off(synth, e.channel, e.key);
        }
        int block = std::min(1024, length - position);
        if (index < count) block = std::min(block, events[index].sample - position);
        if (block <= 0) return 6;
        tsf_render_float(synth, buffer, block, 0);
        fwrite(buffer, sizeof(float) * 2, block, output);
        position += block;
    }
    fclose(output); tsf_close(synth); return 0;
}
